using elasticsearchApi.Utils;
using System.Runtime.Serialization;

namespace elasticsearchApi.Services.Passport
{
    [Serializable]
    public class PassportErrorException : Exception
    {
        private string? _key;
        private string? _message;

        public PassportErrorException()
        {
        }

        public PassportErrorException(string? message)
            : base(message)
        {
        }

        public PassportErrorException(string key, string message)
            : base(message)
        {
            _key = key;
            _message = message;
        }

        public override string Message => !_key.IsNullOrEmpty() ? $"{_key} - {base.Message}" : base.Message;


        public PassportErrorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected PassportErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}