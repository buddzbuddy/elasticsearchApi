using elasticsearchApi.Utils;

namespace elasticsearchApi.Models.Exceptions.CheckExisting
{
    public class CheckExistingException : Exception
    {
        private readonly string _key;
        public CheckExistingException(string key, string message) : base(message) {
            _key = key;
        }

        public override string Message => !_key.IsNullOrEmpty() ? $"{_key} - {base.Message}" : base.Message;
    }
}
