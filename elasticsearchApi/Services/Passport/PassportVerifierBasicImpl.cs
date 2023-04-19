using Elasticsearch.Net;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using System;

namespace elasticsearchApi.Services.Passport
{
    public class PassportVerifierBasicImpl : IPassportVerifierBasic
    {
        public void Verify(modifyPersonPassportDTO passport)
        {
            var passportTypeId = new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}");
            var passportType = passport.passporttype;
            if (passportType == null)
                throw new PassportErrorException("PassportType", "Тип удостоверяющего документа не указан!");
            var isPassport = passportType != null && (Guid)passportType == passportTypeId;

            var v = passport.passportseries;
            var series = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(series))
                throw new PassportErrorException("passportseries", "Серия удостоверяющего документа не указана!");
            v = passport.passportno;
            var no = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(no))
                throw new PassportErrorException("passportno", "Номер удостоверяющего документа не указан!");
            var pDate = passport.date_of_issue;
            if (isPassport && pDate == null)
                throw new PassportErrorException("date_of_issue", "Дата выдачи удостоверяющего документа не указана!");
            var issueDate = pDate;
            v = passport.issuing_authority;
            var authority = v != null ? v.ToString().Trim() : string.Empty;
            if (isPassport && string.IsNullOrWhiteSpace(authority))
                throw new PassportErrorException("issuing_authority", "Орган выдавший удостоверяющий документ не указан!");
            var familyState = passport.familystate;
            if (isPassport && familyState == null)
                throw new PassportErrorException("familystate", "Семейное положение не указано!");
        }
    }
}
