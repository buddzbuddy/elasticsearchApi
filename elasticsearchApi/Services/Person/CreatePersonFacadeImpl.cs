using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Contracts.PinGenerator;
using elasticsearchApi.Models.Exceptions.InMemory;
using elasticsearchApi.Models.Exceptions.Person;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;
using SqlKata.Execution;
using System.Collections.Concurrent;

namespace elasticsearchApi.Services.Person
{
    public class CreatePersonFacadeImpl : ICreatePersonFacade
    {
        private readonly IPinGenerator _pinGenerator;
        private readonly IPersonCreator _personCreator;
        private readonly IAddressRefsVerifier _addressRefsVerifier;
        private readonly AppTransaction _appTransaction;
        private readonly QueryFactory _queryFactory;
        private readonly IInMemoryProvider _inMemoryProvider;
        public CreatePersonFacadeImpl(IPinGenerator pinGenerator, IPersonCreator personCreator,
            IAddressRefsVerifier addressRefsVerifier,
            AppTransaction appTransaction, QueryFactory queryFactory,
            IInMemoryProvider inMemoryProvider)
        {
            _pinGenerator = pinGenerator;
            _personCreator = personCreator;
            _addressRefsVerifier = addressRefsVerifier;
            _appTransaction = appTransaction;
            _queryFactory = queryFactory;
            _inMemoryProvider = inMemoryProvider;
        }
        private static readonly ConcurrentDictionary<int, Lazy<SemaphoreSlim>> _semaphore = new();
        public outPersonDTO CreateNewPerson(addNewPersonDTO dto, in int regionNo, in int districtNo)
        {
            _addressRefsVerifier.Verify(regionNo, districtNo);
            var regCode = regionNo * 1000 + districtNo;
            var sem = _semaphore.GetOrAdd(regCode, _ => new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1))).Value;
            sem.Wait();
            if(_queryFactory.Connection.State != System.Data.ConnectionState.Open)
            {
                _queryFactory.Connection.Open();
            }
            _appTransaction.Transaction ??= _queryFactory.Connection.BeginTransaction();
            try
            {
                var newIin = _pinGenerator.GenerateNewPin(regCode);
                var newPersonId = _personCreator.CreateNewPerson(dto, newIin);
                var model = new outPersonDTO
                {
                    id = newPersonId,
                    iin = newIin,
                    last_name = dto.last_name,
                    first_name = dto.first_name,
                    middle_name = dto.middle_name,
                    date_of_birth = dto.date_of_birth,
                    sex = dto.sex,
                    passporttype = dto.passporttype,
                    passportseries = dto.passportseries,
                    passportno = dto.passportno,
                    issuing_authority = dto.issuing_authority,
                    date_of_issue = dto.date_of_issue,
                    familystate = dto.familystate
                };
                try
                {
                    _inMemoryProvider.Save(model);
                }
                catch (Exception e)
                {
                    throw new SaveInMemoryException("Попытка сохранить гражданина во временную кэш-память", e);
                }
                _appTransaction.OnCommit?.Invoke();
                return model;
            }
            catch (SaveInMemoryException e)
            {
                _appTransaction.OnRollback?.Invoke();
                throw new PersonInsertException($"{e.Message}: inner message {e.GetBaseException().Message}", e);
            }
            catch (Exception e)
            {
                _appTransaction.OnRollback?.Invoke();
                throw new PersonInsertException($"Ошибка при попытке сохранить гражданина в базу данных! {e.GetBaseException().Message}", e);
            }
            finally//will this part of code execute if exception has been thrown in try part?
            {
                sem.Release();
            }
        }
    }
}
