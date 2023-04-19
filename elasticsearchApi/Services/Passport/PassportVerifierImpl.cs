using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using System.Data;

namespace elasticsearchApi.Services.Passport
{
    public class PassportVerifierImpl : IPassportVerifier
    {
        private IPassportVerifierBasic _passportVerifierBasic;
        private IPassportVerifierLogic _passportVerifierLogic;
        private IPassportDbVerifier _passportDbVerifier;
        public PassportVerifierImpl(IPassportVerifierBasic passportVerifierBasic,
            IPassportVerifierLogic passportVerifierLogic,
            IPassportDbVerifier passportDbVerifier)
        {
            _passportVerifierBasic = passportVerifierBasic;
            _passportVerifierLogic = passportVerifierLogic;
            _passportDbVerifier = passportDbVerifier;
        }
        public void VerifyPassport(modifyPersonPassportDTO passport, IDbTransaction? transaction = null)
        {
            _passportVerifierBasic.Verify(passport);
            _passportVerifierLogic.Verify(passport);
            _passportDbVerifier.Verify(passport.passportno, transaction);
        }
    }
}
