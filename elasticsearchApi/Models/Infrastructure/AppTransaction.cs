using SqlKata.Execution;
using System.Data;

namespace elasticsearchApi.Models.Infrastructure
{
    public delegate void OnCommit();
    public delegate void OnRollback();
    public class AppTransaction
    {
        public AppTransaction()
        {
            OnCommit = Commit;
            OnRollback = Rollback;
            Created = DateTime.Now;

        }
        private IDbTransaction? _dbTransaction;
        public IDbTransaction? Transaction
        {
            get
            {
                return _dbTransaction;
            }
            set
            {
                if (_dbTransaction != null)
                {
                    try
                    {
                        _dbTransaction.Rollback();
                    }
                    catch
                    {

                    }
                }
                _dbTransaction = value;
            }
        }

        public void DisableCommitRollback()
        {
            OnCommit = null;
            OnRollback = null;
        }
        public OnCommit? OnCommit;
        public OnRollback? OnRollback;

        public DateTime Created;

        private void Commit()
        {
            Transaction?.Commit();
            Transaction = null;
        }
        private void Rollback()
        {
            Transaction?.Rollback();
            Transaction = null;
        }

    }
}
