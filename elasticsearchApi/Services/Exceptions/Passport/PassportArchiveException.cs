using System.Runtime.Serialization;
using elasticsearchApi.Services.Exceptions.Base;

namespace elasticsearchApi.Services.Exceptions.Passport
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