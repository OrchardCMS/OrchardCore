using System;
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

        public DbConnection CreateConnection()
        {
            // Forward the call to the actual factory
            var connection = _factory.CreateConnection();

            // Reuse the actual dialect if not already defined
            if (!SqlDialectFactory.SqlDialects.ContainsKey(ConnectionName))
            {
                SqlDialectFactory.SqlDialects[ConnectionName] = SqlDialectFactory.SqlDialects[connection.GetType().Name.ToLower()];
            }

            return new ProfiledDbConnection(connection, new CurrentDbProfiler(() => StackExchange.Profiling.MiniProfiler.Current));
        }
    }
}
