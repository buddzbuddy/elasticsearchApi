using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Person;
using Nest;
using System;

namespace elasticsearchApi.Services.Person
{
    public class AddNewPersonVerifierImpl : IAddNewPersonVerifier
    {
        private readonly IPersonBasicVerifier _personBasicVerifier;
        private readonly IPersonLogicVerifier _personLogicVerifier;
        private readonly IPassportVerifier _passportVerifier;
        public AddNewPersonVerifierImpl(IPersonBasicVerifier personBasicVerifier, IPersonLogicVerifier personLogicVerifier,
            IPassportVerifier passportVerifier)
        {
            _personBasicVerifier = personBasicVerifier;
            _personLogicVerifier = personLogicVerifier;
            _passportVerifier = passportVerifier;
        }
        /// <summary>
        /// Includes Passport verify also
        /// </summary>
        /// <param name="person"></param>
        public void VerifyPerson(addNewPersonDTO person)
        {
            _personBasicVerifier.Verify(person);
            _personLogicVerifier.Verify(person);
            _passportVerifier.VerifyPassport(person);
        }
    }
}
