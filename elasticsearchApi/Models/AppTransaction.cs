using System.Data;

namespace elasticsearchApi.Models
{
    public delegate void OnCommit();
    public delegate void OnRollback();
    public class AppTransaction
    {
        public AppTransaction() {
            OnCommit = Commit;
            OnRollback = Rollback;
        }
        public IDbTransaction? Transaction { get; set; }
        public OnCommit? OnCommit;
        public OnRollback? OnRollback;

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
