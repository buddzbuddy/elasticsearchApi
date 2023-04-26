using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Contracts.PinGenerator;
using elasticsearchApi.Models.Person;
using System.Collections.Concurrent;

namespace elasticsearchApi.Services.Person
{
    public class CreatePersonFacadeImpl : ICreatePersonFacade
    {
        private readonly IPinGenerator _pinGenerator;
        private readonly IPersonCreator _personCreator;
        public CreatePersonFacadeImpl(IPinGenerator pinGenerator, IPersonCreator personCreator)
        {
            _pinGenerator = pinGenerator;
            _personCreator = personCreator;
        }
        private static readonly ConcurrentDictionary<int, Lazy<SemaphoreSlim>> _semaphore = new();
        public outPersonDTO CreateNewPerson(addNewPersonDTO dto, in int regionNo, in int districtNo)
        {
            var regCode = regionNo * 1000 + districtNo;
            var sem = _semaphore.GetOrAdd(regCode, _ => new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1))).Value;
            sem.Wait();
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

                return model;
            }
            finally//will this part of code execute if exception has been thrown in try part?
            {
                sem.Release();
            }
        }
    }
}
