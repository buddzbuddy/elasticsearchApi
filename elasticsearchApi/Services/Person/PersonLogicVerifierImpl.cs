using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Exceptions.Person;
using elasticsearchApi.Utils;
using System;
using System.Text.RegularExpressions;

namespace elasticsearchApi.Services.Person
{
    public class PersonLogicVerifierImpl : IPersonLogicVerifier
    {
        private readonly Guid?[] genders
            = StaticReferences.getEnumItems<Genders>()
            .Select(x => x.id).ToArray();
        public void Verify(IPersonData person)
        {
            var nameRegex = new Regex("[0-9]");
            if (!string.IsNullOrEmpty(person.last_name) && nameRegex.IsMatch(person.last_name))
                throw new PersonInputErrorException("Last_Name", "Ошибка в формате фамилии! Должны быть только буквы.");

            if (!string.IsNullOrEmpty(person.first_name) && nameRegex.IsMatch(person.first_name))
                throw new PersonInputErrorException("First_Name", "Ошибка в формате имени! Должны быть только буквы.");

            if (!string.IsNullOrEmpty(person.middle_name) && nameRegex.IsMatch(person.middle_name))
                throw new PersonInputErrorException("Middle_Name", "Ошибка в формате отчества! Должны быть только буквы.");

            if (!string.IsNullOrEmpty(person.iin))
            {
                if (person.iin.Length > 14)
                    throw new PersonInputErrorException("IIN", "Ошибка в длине ПИН");
                var pinRegex = new Regex("[^0-9]");
                if (pinRegex.IsMatch(person.iin))
                    throw new PersonInputErrorException("IIN", "Ошибка в формате номера ПИН! Должны быть только цифры");
            }

            if (person.sex != null && !genders.Contains(person.sex))
                throw new PersonInputErrorException("Sex",
                    $"Пол ({person.sex}) выбран некорректно! Доступны только 2 варианта (муж=74C6C7FE-53C6-4492-A62F-65A7A49AB644, жен=56E07640-5B5B-47FA-832D-A6639F36EB71)");

            if (person.date_of_birth != null && person.date_of_birth.Value > DateTime.Today)
                throw new PersonInputErrorException("Date_of_Birth", "Дата рождения указана некорректно!");
        }
    }
}
