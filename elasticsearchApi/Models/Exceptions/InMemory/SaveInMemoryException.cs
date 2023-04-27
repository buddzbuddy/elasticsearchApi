namespace elasticsearchApi.Models.Exceptions.InMemory
{
    public class SaveInMemoryException : Exception
    {
        public SaveInMemoryException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
