using Elasticsearch.Net;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Passport;
using elasticsearchApi.Utils;
using System;

namespace elasticsearchApi.Services.Passport
{
    public class PassportVerifierLogicImpl : IPassportVerifierLogic
    {
        private readonly Guid?[] familystateList
            = StaticReferences.getEnumItems<FamilyStates>()
            .Select(x => x.id).ToArray();
        private readonly Guid?[] passporttypeList
            = StaticReferences.getEnumItems<PassportTypes>()
            .Select(x => x.id).ToArray();
        public void Verify(IPassportData passport)
        {
            if (passport.passporttype != null
                && !passporttypeList.Contains(passport.passporttype))
            {
                throw new PassportInputErrorException(
                    "passporttype",
                    "Тип удостоверяющего документа не распознан!");
            }
            if (passport.familystate != null
                && !familystateList.Contains(passport.familystate))
            {
                throw new PassportInputErrorException(
                    "familystate",
                    "Семейное положение не распознано!");
            }
        }
    }
}
