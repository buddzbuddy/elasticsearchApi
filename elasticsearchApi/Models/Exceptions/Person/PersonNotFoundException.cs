using System.Runtime.Serialization;
using elasticsearchApi.Models.Exceptions.Base;

namespace elasticsearchApi.Models.Exceptions.Person
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