using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Services.Exceptions;
using SqlKata.Execution;
using System.Data;

namespace elasticsearchApi.Services.Passport
{
    public class ModifyPassportActorImpl : IModifyPassportActor
    {
        private readonly IModifyPassportDataService _dataSvc;
        public ModifyPassportActorImpl(IModifyPassportDataService dataService)
        {
            _dataSvc = dataService;
        }
        public IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person)
        {
            IServiceContext context = new ServiceContext();
            IDbTransaction? transaction = null;
            try
            {
                _dataSvc.Execute(iin, person, transaction);
                context.SuccessFlag = true;
            }
            catch (Exception e) when
            (
            e is PersonNotFoundException ||
            e is PassportDuplicateException ||
            e is PersonNotFoundException ||
            e is PassportArchiveException ||
            e is PersonUpdateException
            )
            {
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("type", "Выполнено обработанное исключение");
            }
            catch (Exception e)
            {
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("type", "ModifyPassportDataService - Выполнено необработанное исключение");
                context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
            }
            finally
            {
                if (!context.SuccessFlag && transaction != null)
                {
                    transaction.Rollback();
                }
            }
            return context;
        }
    }
}
