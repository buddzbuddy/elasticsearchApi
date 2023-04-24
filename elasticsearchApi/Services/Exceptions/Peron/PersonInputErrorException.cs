using elasticsearchApi.Services.Exceptions.Base;
using elasticsearchApi.Utils;
using System.Runtime.Serialization;

namespace elasticsearchApi.Services.Exceptions.Passport
{
    public class PersonInputErrorException : PassportInputErrorException
    {
        public PersonInputErrorException(string key, string message) : base(key, message)
        {
        }
    }
}