using System.Data;
using System.Data.Common;
using StackExchange.Profiling.Data;
using YesSql;

namespace OrchardCore.MiniProfiler
{
    internal class MiniProfilerConnectionFactory : IConnectionFactory
    {
        private readonly IConnectionFactory _factory;

        public MiniProfilerConnectionFactory(IConnectionFactory factory)
        {
            _factory = factory;
        }

        public void CloseConnection(IDbConnection connection)
        {
            var profiledConnection = connection as ProfiledDbConnection;
            if (profiledConnection != null)
            {
                _factory.CloseConnection(profiledConnection.WrappedConnection);
            }
        }

        public IDbConnection CreateConnection()
        {
            var connection = (DbConnection)_factory.CreateConnection();
            SqlDialectFactory.SqlDialects[nameof(ContextProfiledDbConnection).ToLower()] = SqlDialectFactory.SqlDialects[connection.GetType().Name.ToLower()];
            return new ContextProfiledDbConnection(connection, StackExchange.Profiling.MiniProfiler.DefaultOptions.ProfilerProvider);
        }

        public void Dispose()
        {
            _factory.Dispose();
        }
    }
}
