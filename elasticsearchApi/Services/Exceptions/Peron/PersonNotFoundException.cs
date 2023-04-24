using System.Runtime.Serialization;
using elasticsearchApi.Services.Exceptions.Base;

namespace elasticsearchApi.Services.Exceptions.Peron
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