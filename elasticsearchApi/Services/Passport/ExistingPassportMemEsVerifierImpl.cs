using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Person;

namespace elasticsearchApi.Services.Passport
{
    public class ExistingPassportMemEsVerifierImpl : IExistingPassportVerifier
    {
        private readonly ICheckFacade _checkFacade;
        public ExistingPassportMemEsVerifierImpl(ICheckFacade checkFacade)
        {
            _checkFacade= checkFacade;
        }
        public void CheckExistingPassportByNo(string passportNo, int? excludePersonId = null)
        {
            var filter = new Dictionary<string, object>()
            {
                { "passportno", passportNo }
            };
            outPersonDTO existingPerson;
            if (excludePersonId != null && excludePersonId > 0)
            {
                var excludeFilter = new Dictionary<string, object>()
                {
                    { "id", excludePersonId }
                };
                filter.Add("id", excludePersonId);
                existingPerson = _checkFacade.CallCheck(filter, excludeFilter);
            }
            else
            {
                existingPerson = _checkFacade.CallCheck(filter);
            }

            if (existingPerson != null)
            {
                var msg = $"Найден дубликат по паспорту! Данный номер ({passportNo}) паспорта принадлежит существующему гражданину ПИН: {existingPerson.iin}, ФИО: {existingPerson.last_name} {existingPerson.first_name} {existingPerson.middle_name}, Номер паспорта: {existingPerson.passportno}";
                throw new PassportDuplicateException(msg);
            }
        }
    }
}
