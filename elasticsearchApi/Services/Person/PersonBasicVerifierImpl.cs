using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services.Exceptions.Passport;
using System;

namespace elasticsearchApi.Services.Person
{
    public class PersonBasicVerifierImpl : IPersonBasicVerifier
    {
        public void Verify(IPersonData person)
        {
            if (string.IsNullOrEmpty(person.last_name))
                throw new PassportInputErrorException("Last_Name", "Заполните фамилию");

            if (string.IsNullOrEmpty(person.first_name))
                throw new PassportInputErrorException("First_Name", "Заполните имя");

            if (person.sex == null)
                throw new PassportInputErrorException("Sex", "Введите пол");

            if (person.date_of_birth == null)
                throw new PassportInputErrorException("Date_of_Birth", "Введите дату рождения");
        }
    }
}
