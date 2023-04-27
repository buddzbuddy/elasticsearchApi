using elasticsearchApi.Contracts.Person;
using elasticsearchApi.Models.Exceptions.Person;
using elasticsearchApi.Models.Infrastructure;
using elasticsearchApi.Models.Person;
using elasticsearchApi.Utils;
using SqlKata.Execution;
using System;

namespace elasticsearchApi.Services.Person
{
    public class PersonCreatorImpl : IPersonCreator
    {
        private readonly QueryFactory _queryFactory;
        private readonly AppTransaction _appTransaction;
        public PersonCreatorImpl(QueryFactory queryFactory, AppTransaction appTransaction)
        {
            _queryFactory = queryFactory;
            _appTransaction = appTransaction;
        }
        public int CreateNewPerson(addNewPersonDTO dto, string newIin)
        {
            dto.iin = newIin;
            int newPersonId = 0;
            try
            {
                newPersonId = _queryFactory.Query("Persons").InsertGetId<int>(dto, _appTransaction.Transaction);
                _appTransaction.OnCommit?.Invoke();
            }
            catch (Exception e)
            {
                _appTransaction.OnRollback?.Invoke();
                throw new PersonInsertException($"Ошибка при попытке сохранить гражданина в базу данных! {e.GetBaseException().Message}", e);
            }

            return newPersonId;
        }
    }
}
