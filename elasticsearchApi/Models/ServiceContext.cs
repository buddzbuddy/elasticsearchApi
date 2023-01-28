using System.Collections.Generic;

namespace elasticsearchApi.Models
{
    public class ServiceContext
    {
        public bool SuccessFlag { get; set; }

        public IDictionary<string, string> ErrorMessages { get; set; } = new Dictionary<string, string>();

        internal void AddErrorMessage(string key, string errorMessage)
        {
            if (!ErrorMessages.ContainsKey(key))
                ErrorMessages.Add(key, errorMessage);
            else
                ErrorMessages[key] += "; " + errorMessage;
        }

        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        internal object this[string attributeName]
        {
            get
            {
                return Data.ContainsKey(attributeName) ? Data[attributeName] : null;
            }
            set
            {
                if (Data.ContainsKey(attributeName)) Data[attributeName] = value;
                else Data.Add(attributeName, value);
            }
        }
    }
}
