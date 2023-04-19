using System.Runtime.Serialization;

namespace elasticsearchApi.Services.Passport
{
    [Serializable]
    public class PersonUpdateException : Exception
    {
        public PersonUpdateException()
        {
        }

        public PersonUpdateException(string? message) : base(message)
        {
        }

        public PersonUpdateException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected PersonUpdateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}