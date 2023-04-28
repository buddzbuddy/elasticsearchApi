namespace elasticsearchApi.Contracts.Passport
{
    public interface IExistingPassportVerifier
    {
        void CheckExistingPassportByNo(string passportNo, int? excludePersonId = null);
    }
}
