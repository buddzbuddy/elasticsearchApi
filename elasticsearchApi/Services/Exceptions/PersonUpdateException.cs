using System.Runtime.Serialization;

namespace elasticsearchApi.Services.Exceptions
{
    [Serializable]
    public class PersonUpdateException : Exception, IWriteException
    {
        public PersonUpdateException(string? message) : base(message)
        {
        }

        public string ExceptionType => nameof(PersonUpdateException);
    }
}