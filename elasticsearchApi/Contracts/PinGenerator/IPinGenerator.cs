namespace elasticsearchApi.Contracts.PinGenerator
{
    public interface IPinGenerator
    {
        long GenerateNewPin(int regionNo, int districtNo);
    }
}
