using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models.Contracts;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Services.Exceptions.Passport;
using System;
using System.Text.RegularExpressions;

namespace elasticsearchApi.Services.Person
{
    public class PersonLogicVerifierImpl : IPersonLogicVerifier
    {
        private readonly Guid[] genders = new[]
        {
            new Guid("74C6C7FE-53C6-4492-A62F-65A7A49AB644"),//Male
            new Guid("56E07640-5B5B-47FA-832D-A6639F36EB71"),//Female
        };
        public void Verify(IPersonData person)
        {
            var nameRegex = new Regex("[0-9]");
            if (!string.IsNullOrEmpty(person.last_name) && nameRegex.IsMatch(person.last_name))
                throw new PassportInputErrorException("Last_Name", "Ошибка в формате фамилии! Должны быть только буквы.");

            if (!string.IsNullOrEmpty(person.first_name) && nameRegex.IsMatch(person.first_name))
                throw new PassportInputErrorException("First_Name", "Ошибка в формате имени! Должны быть только буквы.");

            if (!string.IsNullOrEmpty(person.middle_name) && nameRegex.IsMatch(person.middle_name))
                throw new PassportInputErrorException("Middle_Name", "Ошибка в формате отчества! Должны быть только буквы.");

            if (!string.IsNullOrEmpty(person.iin))
            {
                if (person.iin.Length > 14)
                    throw new PassportInputErrorException("IIN", "Ошибка в длине ПИН");
                var pinRegex = new Regex("[^0-9]");
                if (pinRegex.IsMatch(person.iin))
                    throw new PassportInputErrorException("IIN", "Ошибка в формате номера ПИН! Должны быть только цифры");
            }

            if (person.sex != null && genders.Contains(person.sex.Value))
                throw new PassportInputErrorException("Sex",
                    $"Пол ({person.sex}) выбран некорректно! Доступны только 2 варианта (муж=74C6C7FE-53C6-4492-A62F-65A7A49AB644, жен=56E07640-5B5B-47FA-832D-A6639F36EB71)");

            if (person.date_of_birth != null && person.date_of_birth.Value > DateTime.Today)
                throw new PassportInputErrorException("Date_of_Birth", "Введите дату рождения");
        }
    }
}
