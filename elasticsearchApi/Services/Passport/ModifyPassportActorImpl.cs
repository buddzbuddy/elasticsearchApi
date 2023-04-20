using elasticsearchApi.Contracts;
using elasticsearchApi.Contracts.Passport;
using elasticsearchApi.Models;
using elasticsearchApi.Services.Exceptions;
using SqlKata.Execution;
using System.Data;
using System.Threading;

namespace elasticsearchApi.Services.Passport
{
    public delegate void OnSuccess(IDbTransaction transaction);
    public delegate void OnFailure(IDbTransaction transaction);
    public class ModifyPassportActorImpl : IModifyPassportActor
    {
        private readonly IModifyPassportDataService _dataSvc;
        public ModifyPassportActorImpl(IModifyPassportDataService dataService)
        {
            _dataSvc = dataService;
        }
        public IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person, IDbTransaction? transaction = null) => CallModifyPassport(iin, person, ref transaction);
        public IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person, ref IDbTransaction? transaction)
        {
            return CallModifyPassport(iin, person, Commit, Rollback, ref transaction);
        }
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public IServiceContext CallModifyPassport(string iin, modifyPersonPassportDTO person, OnSuccess? onSuccess, OnFailure? onFailure, ref IDbTransaction? transaction)
        {
            IServiceContext context = new ServiceContext();
            try
            {
                semaphore.Wait();
                _dataSvc.Execute(iin, person, ref transaction);
                context.SuccessFlag = true;
            }
            catch (Exception e) when
            (
            e is PassportInputErrorException ||
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
                if (transaction != null)
                {
                    if (context.SuccessFlag)
                    {

                        //transaction.Commit();
                        onSuccess?.Invoke(transaction);
                    }
                    else
                    {
                        //transaction.Rollback();
                        onFailure?.Invoke(transaction);
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
