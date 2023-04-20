using System.Runtime.Serialization;

namespace elasticsearchApi.Services.Exceptions
{
    [Serializable]
    public class PassportDuplicateException : Exception, IReadException
    {
        public string ExceptionType { get { return nameof(PassportDuplicateException); } }

        public PassportDuplicateException(string? message) : base(message)
        {
        }
    }
}