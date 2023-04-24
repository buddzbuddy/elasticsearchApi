using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Passport;
using elasticsearchApi.Services.Exceptions.Passport;
using elasticsearchApi.Services.Exceptions.Peron;
using SqlKata.Execution;
using System.Data;
using System.Threading;

namespace elasticsearchApi.Services.Passport
{
    public class ModifyPassportActorImpl : IModifyPassportActor
    {
        private readonly AppTransaction appTransaction;
        private readonly IModifyPassportDataService _dataSvc;
        public ModifyPassportActorImpl(IModifyPassportDataService dataService, AppTransaction appTransaction)
        {
            _dataSvc = dataService;
            this.appTransaction = appTransaction;
        }
        private SemaphoreSlim semaphore = new (1, 1);
        public IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person)
        {
            IServiceContext context = new ServiceContext();
            try
            {
                semaphore.Wait();
                _dataSvc.Execute(iin, person);
                context.SuccessFlag = true;
            }
            catch (Exception e) when
            (
            e is PersonInputErrorException ||
            e is PersonNotFoundException ||
            e is PassportDuplicateException ||
            e is PassportArchiveException ||
            e is PersonUpdateException
            )
            {
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("type", "ModifyPassportActor - Выполнено обработанное исключение");
            }
            catch (Exception e)
            {
                context.AddErrorMessage("errorMessage", e.GetBaseException().Message);
                context.AddErrorMessage("type", "ModifyPassportDataService - Выполнено необработанное исключение");
                context.AddErrorMessage("errorTrace", e.StackTrace ?? "");
            }
            finally
            {
                if (appTransaction.Transaction != null)
                {
                    if (context.SuccessFlag)
                    {
                        appTransaction.OnCommit?.Invoke();
                    }
                    else
                    {
                        appTransaction.OnRollback?.Invoke();
                    }
                }
                semaphore.Release();
            }
            return context;
        }

        private void Commit(IDbTransaction transaction)
        {
            transaction.Commit();
        }

        private void Rollback(IDbTransaction transaction)
        {
            transaction.Rollback();
        }
    }
}
