using System.Data;
using System.Data.Common;

namespace Orchard.Data.Diagnostics
{
    public class DiagnosticDbTransaction : DbTransaction
    {
        private readonly DbTransaction _transaction; 
        public DiagnosticDbTransaction(DbTransaction transaction)
        {
            _transaction = transaction;
        }

        public override IsolationLevel IsolationLevel => _transaction.IsolationLevel;
        protected override DbConnection DbConnection => _transaction.Connection;

        public override void Commit()
        {
            _transaction.Commit();
        }

        public override void Rollback()
        {
            _transaction.Rollback();
        }
    }
}
