using elasticsearchApi.Models;
using Nest;

namespace elasticsearchApi.Services
{
    public interface IDataVerifier
    {
        bool Verify(modifyPersonPassportDTO person, out Dictionary<string, string> errors);
    }

    public class DataVerifier : IDataVerifier
    {
        public bool Verify(modifyPersonPassportDTO person, out Dictionary<string, string> errors)
        {
            errors = new Dictionary<string, string>();
            var passportTypeId = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}");
            var passportType = person.passporttype;
            if (passportType == null)
                errors.Add("PassportType", "Тип удостоверяющего документа не указан!");
            var isPassport = passportType != null && (Guid)passportType == passportTypeId;

            var v = person.passportseries;
            var series = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(series))
                errors.Add("passportseries", "Серия удостоверяющего документа не указана!");
            v = person.passportno;
            var no = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(no))
                errors.Add("passportno", "Номер удостоверяющего документа не указан!");
            var pDate = person.date_of_issue;
            if (isPassport && pDate == null)
                errors.Add("date_of_issue", "Дата выдачи удостоверяющего документа не указана!");
            var issueDate = pDate;
            v = person.issuing_authority;
            var authority = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(authority))
                errors.Add("issuing_authority", "Орган выдавший удостоверяющий документ не указан!");
            var familyState = person.familystate;
            if (isPassport && familyState == null)
                errors.Add("familystate", "Семейное положение не указано!");
            return errors.Any();
        }
    }
}
