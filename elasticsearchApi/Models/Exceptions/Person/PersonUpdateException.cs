using System.Runtime.Serialization;
using elasticsearchApi.Models.Exceptions.Base;

namespace elasticsearchApi.Models.Exceptions.Person
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