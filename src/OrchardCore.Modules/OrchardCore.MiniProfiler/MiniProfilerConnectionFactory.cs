using System;
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

        public Type DbConnectionType => typeof(ProfiledDbConnection);

        public DbConnection CreateConnection()
        {
            // Forward the call to the actual factory
            var connection = _factory.CreateConnection();

            return new ProfiledDbConnection(connection, new CurrentDbProfiler(() => StackExchange.Profiling.MiniProfiler.Current));
        }
    }
}
