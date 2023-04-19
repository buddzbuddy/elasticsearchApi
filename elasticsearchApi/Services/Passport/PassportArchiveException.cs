using System.Runtime.Serialization;

namespace elasticsearchApi.Services.Passport
{
    [Serializable]
    public class PassportArchiveException : Exception
    {
        public PassportArchiveException()
        {
        }

        public PassportArchiveException(string? message) : base(message)
        {
        }

        public PassportArchiveException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected PassportArchiveException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}