using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.DataProviders;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Exceptions.Person;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Passport;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Utils;
using SqlKata.Execution;
using System.Data;
using System.Threading;

namespace elasticsearchApi.Services.Passport
{
    public class ModifyPassportActorImpl : IModifyPassportActor
    {
        private readonly AppTransaction _appTransaction;
        private readonly IModifyPassportDataService _dataSvc;
        private readonly IInMemoryProvider _inMemoryProvider;
        public ModifyPassportActorImpl(IModifyPassportDataService dataService, AppTransaction appTransaction,
            IInMemoryProvider inMemoryProvider)
        {
            _dataSvc = dataService;
            _appTransaction = appTransaction;
            _inMemoryProvider = inMemoryProvider;
        }
        private SemaphoreSlim semaphore = new (1, 1);
        public IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person)
        {
            IServiceContext context = new ServiceContext();
            var personOut = new outPersonDTO
            {
                passportno = person.passportno,
                passportseries= person.passportseries,
                passporttype= person.passporttype,
                date_of_issue= person.date_of_issue,
                issuing_authority = person.issuing_authority,
                familystate= person.familystate,
                iin= iin
            };
            semaphore.Wait();
            try
            {
                _dataSvc.Execute(iin, person, personOut);
                context.SuccessFlag = true;
            }
            catch (Exception e) when
            (
            e is PassportInputErrorException ||
            e is PersonNotFoundException ||
            e is PassportDuplicateException ||
            e is PassportArchiveException ||
            e is PersonUpdateException
            )
            {
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("type", "ModifyPassportActor - Выполнено обработанное исключение");
            }
            catch (Exception e)
            {
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("type", "ModifyPassportDataService - Выполнено необработанное исключение");
                context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
            }
            finally
            {
                if (context.SuccessFlag)
                {
                    _inMemoryProvider.Save(personOut);
                    _appTransaction.OnCommit?.Invoke();
                }
                else
                {
                    _appTransaction.OnRollback?.Invoke();
                }
                semaphore.Release();
            }
            return context;
        }
    }
}
