using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Passport;
using System.Data;

namespace elasticsearchApi.Services.Passport
{
    public class PassportVerifierImpl : IPassportVerifier
    {
        private IPassportVerifier _passportVerifierBasic;
        private IPassportVerifierLogic _passportVerifierLogic;
        private IPassportDbVerifier _passportDbVerifier;
        public PassportVerifierImpl(IPassportVerifier passportVerifierBasic,
            IPassportVerifierLogic passportVerifierLogic,
            IPassportDbVerifier passportDbVerifier)
        {
            _passportVerifierBasic = passportVerifierBasic;
            _passportVerifierLogic = passportVerifierLogic;
            _passportDbVerifier = passportDbVerifier;
        }
        public void VerifyPassport(IPassportData passport, IDbTransaction? transaction = null)
        {
            _passportVerifierBasic.Verify(passport);
            _passportVerifierLogic.Verify(passport);
            //_passportDbVerifier.Verify(passport.passportno, transaction);
        }
    }
}
