using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Exceptions.Person;
using elasticsearchApi.Models.Person;
using System;

namespace elasticsearchApi.Services.Person
{
    public class PersonBasicVerifierImpl : IPersonBasicVerifier
    {
        public void Verify(IPersonData person)
        {
            if (string.IsNullOrEmpty(person.last_name))
                throw new PersonInputErrorException("Last_Name", "Заполните фамилию");

            if (string.IsNullOrEmpty(person.first_name))
                throw new PersonInputErrorException("First_Name", "Заполните имя");

            if (person.sex == null)
                throw new PersonInputErrorException("Sex", "Введите пол");

            if (person.date_of_birth == null)
                throw new PersonInputErrorException("Date_of_Birth", "Введите дату рождения");
        }
    }
}
