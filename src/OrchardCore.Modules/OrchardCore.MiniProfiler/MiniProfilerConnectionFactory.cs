using System;
using System.Data;
using System.Data.Common;
using StackExchange.Profiling.Data;
using YesSql;

namespace OrchardCore.MiniProfiler
{
    internal class MiniProfilerConnectionFactory : IConnectionFactory
    {
        private readonly IConnectionFactory _factory;
        private readonly static string ConnectionName = nameof(ProfiledDbConnection).ToLower();

        public Type DbConnectionType => typeof(ProfiledDbConnection);

        public MiniProfilerConnectionFactory(IConnectionFactory factory)
        {
            _factory = factory;
        }

        public void CloseConnection(IDbConnection connection)
        {
            if (connection is ProfiledDbConnection profiledConnection)
            {
                _factory.CloseConnection(profiledConnection.WrappedConnection);
            }
        }

        public IDbConnection CreateConnection()
        {
            // Forward the call to the actual factory
            var connection = (DbConnection)_factory.CreateConnection();

            // Reuse the actual dialect if not already defined
            if (!SqlDialectFactory.SqlDialects.ContainsKey(ConnectionName))
            {
                SqlDialectFactory.SqlDialects[ConnectionName] = SqlDialectFactory.SqlDialects[connection.GetType().Name.ToLower()];
            }
            
            return new ProfiledDbConnection(connection, new CurrentDbProfiler(() => StackExchange.Profiling.MiniProfiler.Current));
        }

        public void Dispose()
        {
            _factory.Dispose();
        }
    }
}
