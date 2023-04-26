using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Contracts.PinGenerator;
using elasticsearchApi.Models.Person;

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
        public outPersonDTO CreateNewPerson(addNewPersonDTO dto, in int regionNo, in int districtNo)
        {
            var newIin = _pinGenerator.GenerateNewPin(regionNo, districtNo);
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
    }
}
