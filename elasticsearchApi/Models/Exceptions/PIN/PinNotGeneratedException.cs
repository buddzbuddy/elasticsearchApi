namespace elasticsearchApi.Models.Exceptions.PIN
{
    public class PinNotGeneratedException : Exception
    {
        public PinNotGeneratedException(string? message)
            : base(message)
        {

        }
    }
}
