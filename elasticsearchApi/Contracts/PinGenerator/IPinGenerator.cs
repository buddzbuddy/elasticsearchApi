namespace elasticsearchApi.Contracts.PinGenerator
{
    public interface IPinGenerator
    {
        string GenerateNewPin(in int regionNo, in int districtNo);
    }
}
