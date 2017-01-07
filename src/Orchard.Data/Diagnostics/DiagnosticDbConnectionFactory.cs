using System;
using System.Data.Common;
using YesSql.Core.Services;

namespace Orchard.Data.Diagnostics
{
    public class DiagnosticDbConnectionFactory<TDbConnection> : DbConnectionFactory<TDbConnection>, 
        IConnectionFactory, IDisposable where TDbConnection : DbConnection, new()
    {
        public DiagnosticDbConnectionFactory(
            string connectionString, bool reuseConnection = false) : base(connectionString, reuseConnection)
        {
        }

        public new DbConnection CreateConnection()
        {
            var connection = base.CreateConnection();
            return new DiagnosticDbConnection(connection);
        }
    }
}
