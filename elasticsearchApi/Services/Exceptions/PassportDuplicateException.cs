using System.Runtime.Serialization;

namespace elasticsearchApi.Services.Exceptions
{
    [Serializable]
    public class PassportDuplicateException : Exception
    {
        public PassportDuplicateException()
        {
        }

        public PassportDuplicateException(string? message) : base(message)
        {
        }

        public PassportDuplicateException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected PassportDuplicateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}