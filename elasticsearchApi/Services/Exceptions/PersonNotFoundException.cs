using System.Runtime.Serialization;

namespace elasticsearchApi.Services.Exceptions
{
    [Serializable]
    public class PersonNotFoundException : Exception, IReadException
    {
        public PersonNotFoundException(string? message) : base(message)
        {
        }

        public string ExceptionType => nameof(PersonNotFoundException);
    }
}