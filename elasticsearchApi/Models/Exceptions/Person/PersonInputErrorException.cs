using elasticsearchApi.Models.Exceptions.Passport;
namespace elasticsearchApi.Models.Exceptions.Person
{
    public class PersonInputErrorException : PassportInputErrorException
    {
        public PersonInputErrorException(string key, string message) : base(key, message)
        {
        }
    }
}