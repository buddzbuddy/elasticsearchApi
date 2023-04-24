namespace elasticsearchApi.Contracts.Infrastructure
{
    public interface IAddressRefsVerifier
    {
        void Verify(int regionNo, int districtNo);
    }
}
