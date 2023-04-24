using Elasticsearch.Net;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Passport;
using elasticsearchApi.Services.Exceptions;
using System;

namespace elasticsearchApi.Services.Passport
{
    public class PassportVerifierLogicImpl : IPassportVerifierLogic
    {
        private readonly Guid[] familystateList = new Guid[]
        {
            new Guid("{6831E05F-9E2F-46DE-AED0-2DE69A8F87D3}"),//Married
            new Guid("{3C58D432-147C-491A-A34A-4A88B2CCBCB5}"),//Single
            new Guid("{2900D318-9207-4241-9CD0-A0B6D6DBC75F}"),//Widower/Widow
            new Guid("{EF783D94-0418-4ABA-B653-6DB2A10E4B92}"),//Divorced
        };
        private readonly Guid[] passporttypeList = new Guid[]
        {
            new Guid("{A77C7DB9-C27F-4FFC-BFC0-0C6959731B98}"),//Passport
            new Guid("{A52BE3AF-5DFA-405B-A4E6-18A64C24F9A5}"),//BirthCertificate
        };
        public void Verify(modifyPersonPassportDTO passport)
        {
            if (passport.passporttype != null
                && !passporttypeList.Contains(passport.passporttype.Value))
            {
                throw new PassportInputErrorException(
                    "passporttype",
                    "Тип удостоверяющего документа не распознан!");
            }
            if (passport.familystate != null
                && !familystateList.Contains(passport.familystate.Value))
            {
                throw new PassportInputErrorException(
                    "familystate",
                    "Семейное положение не распознано!");
            }
        }
    }
}
