namespace elasticsearchApi.Models.Exceptions.Person
{
    public class PersonInsertException : Exception
    {
        public PersonInsertException(string? message, Exception innerException):base(message, innerException) { }
    }
}
