using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;

namespace elasticsearchApi.Services.Passport
{
    public class PassportVerifierImpl : IPassportVerifier
    {
        private IPassportVerifierBasic _passportVerifierBasic;
        private IPassportVerifierLogic _passportVerifierLogic;
        public PassportVerifierImpl(IPassportVerifierBasic passportVerifierBasic,
            IPassportVerifierLogic passportVerifierLogic)
        {
            _passportVerifierBasic = passportVerifierBasic;
            _passportVerifierLogic = passportVerifierLogic;
        }
        public void VerifyPassport(modifyPersonPassportDTO passport)
        {
            _passportVerifierBasic.Verify(passport);
            _passportVerifierLogic.Verify(passport);
        }
    }
}
