using System.Runtime.Serialization;

namespace elasticsearchApi.Services.Exceptions
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