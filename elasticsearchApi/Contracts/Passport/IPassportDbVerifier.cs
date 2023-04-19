using System.Data;

namespace elasticsearchApi.Contracts.Passport
{
    public interface IPassportDbVerifier
    {
        void Verify(in string? passportno, IDbTransaction? transaction = null);
    }
}
