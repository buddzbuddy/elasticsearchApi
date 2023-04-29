using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Nest;
using System;
using System.Collections.Generic;

namespace elasticsearchApi.Models.Infrastructure
{
    public interface IServiceContext
    {
        bool SuccessFlag { get; set; }
        IDictionary<string, object> Data { get; set; }
        object this[string attributeName] { get; set; }
        IDictionary<string, string> ErrorMessages { get; set; }
        void AddErrorMessage(string key, string errorMessage);
        void PatchFromDictionary(IDictionary<string, string> erros);
    }
    public class ServiceContext : IServiceContext
    {
        public bool SuccessFlag { get; set; } = false;

        public IDictionary<string, string> ErrorMessages { get; set; } = new Dictionary<string, string>();

        public void AddErrorMessage(string key, string errorMessage)
        {
            Console.WriteLine($"{key}-{errorMessage}");
            if (!ErrorMessages.ContainsKey(key))
                ErrorMessages.Add(key, errorMessage);
            else
                ErrorMessages[key] += "; " + errorMessage;
        }

        public void PatchFromDictionary(IDictionary<string, string> erros)
        {
            foreach (var item in erros)
            {
                if (!ErrorMessages.ContainsKey(item.Key))
                    ErrorMessages.Add(item.Key, item.Value);
                else
                    ErrorMessages[item.Key] += "; " + item.Value;
            }
        }

        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public object this[string attributeName]
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
