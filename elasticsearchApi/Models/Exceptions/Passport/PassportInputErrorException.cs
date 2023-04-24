using elasticsearchApi.Models.Exceptions.Base;
using elasticsearchApi.Utils;
using System.Runtime.Serialization;

namespace elasticsearchApi.Models.Exceptions.Passport
{
    [Serializable]
    public class PassportInputErrorException : Exception, IReadException
    {
        private string? _key;
        public PassportInputErrorException(string key, string message)
            : base(message)
        {
            _key = key;
        }

        public override string Message => !_key.IsNullOrEmpty() ? $"{_key} - {base.Message}" : base.Message;

        public string ExceptionType => nameof(PassportInputErrorException);
    }
}