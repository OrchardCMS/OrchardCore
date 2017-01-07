using System;
using System.Data;
using System.Data.Common;

namespace Orchard.Data.Diagnostics
{
    public class DiagnosticDbTransaction : DbTransaction
    {
        private readonly DbTransaction _transaction;
        private readonly DiagnosticDbConnection _connection;
        public DiagnosticDbTransaction(DbTransaction transaction, DiagnosticDbConnection connection)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _transaction = transaction;
            _connection = connection;
        }

        public override IsolationLevel IsolationLevel => _transaction.IsolationLevel;
        protected override DbConnection DbConnection => _connection;
        public DbTransaction WrappedTransaction => _transaction;

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
