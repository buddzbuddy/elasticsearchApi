using System.Runtime.Serialization;
using elasticsearchApi.Services.Exceptions.Base;

namespace elasticsearchApi.Services.Exceptions.Peron
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