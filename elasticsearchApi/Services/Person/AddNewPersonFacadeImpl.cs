using elasticsearchApi.Contracts.CheckProviders;
using elasticsearchApi.Contracts.Infrastructure;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models;
using elasticsearchApi.Models.Exceptions.CheckExisting;
using elasticsearchApi.Models.Exceptions.Passport;
using elasticsearchApi.Models.Exceptions.Person;
using elasticsearchApi.Models.Exceptions.PIN;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Utils;
using Nest;
using System;

namespace elasticsearchApi.Services.Person
{
    public class AddNewPersonFacadeImpl : IAddNewPersonFacade
    {
        private readonly IAddNewPersonVerifier _personVerifier;
        private readonly ICheckFacade _checkFacade;
        private readonly ICreatePersonFacade _createPersonFacade;
        private readonly IExistingPassportVerifier _existingPassportVerifier;
        public AddNewPersonFacadeImpl(IAddNewPersonVerifier personVerifier,
            ICheckFacade checkFacade, ICreatePersonFacade createPersonFacade,
            IExistingPassportVerifier existingPassportVerifier)
        {
            _personVerifier = personVerifier;
            _checkFacade = checkFacade;
            _createPersonFacade = createPersonFacade;
            _existingPassportVerifier = existingPassportVerifier;
        }
        public IServiceContext AddNewPerson(addNewPersonDTO dto, in int regionNo, in int districtNo)
        {
            var context = new ServiceContext();
            try
            {
                _personVerifier.VerifyPerson(dto);

                var existingPerson = _checkFacade.CallCheck(BaseService.ModelToDict(dto));
                if (existingPerson != null)
                {
                    context["NewPIN"] = existingPerson.iin ?? "";
                    context["Result"] = existingPerson;
                    context["ResultPIN"] = existingPerson.iin ?? "";
                    context["IsNew"] = false;
                    context.SuccessFlag = true;
                    return context;
                }

                _existingPassportVerifier.CheckExistingPassportByNo(dto.passportno);

                var newPerson = _createPersonFacade.CreateNewPerson(dto, regionNo, districtNo);
                var newPin = newPerson.iin;
                if (string.IsNullOrEmpty(newPin))
                    throw new PinNotGeneratedException("Пин не сгенерировался");

                context["NewPIN"] = newPin;
                context["Result"] = newPerson;
                context["ResultPIN"] = newPin;
                context["IsNew"] = true;
                context.SuccessFlag = true;
            }
            catch (Exception e) when
            (
            e is PassportInputErrorException or
            PersonInputErrorException or
            CheckExistingException or
            PinNotGeneratedException or
            PassportDuplicateException or
            PersonInsertException
            )
            {
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("type", "AddNewPersonFacade - Выполнено обработанное исключение");
                context.AddErrorMessage("errorTrace", e.StackTrace ?? "");

            }
            catch (Exception e)
            {
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("type", "AddNewPersonFacade - Выполнено необработанное исключение");
                context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
            }
            return context;
        }
    }
}
