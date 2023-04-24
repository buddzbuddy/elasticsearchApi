using System.Runtime.Serialization;
using elasticsearchApi.Models.Exceptions.Base;

namespace elasticsearchApi.Models.Exceptions.Passport
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