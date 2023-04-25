using System.Runtime.Serialization;
using elasticsearchApi.Models.Exceptions.Base;

namespace elasticsearchApi.Models.Exceptions.Passport
{
    [Serializable]
    public class PassportArchiveException : Exception, IWriteException
    {
        public PassportArchiveException(string? message) : base(message)
        {
        }

        public string ExceptionType => nameof(PassportArchiveException);
    }
}