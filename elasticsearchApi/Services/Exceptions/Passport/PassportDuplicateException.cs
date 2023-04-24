using System.Runtime.Serialization;
using elasticsearchApi.Services.Exceptions.Base;

namespace elasticsearchApi.Services.Exceptions.Passport
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