using System;
using System.Data.Common;
using StackExchange.Profiling.Data;
using YesSql;

namespace OrchardCore.MiniProfiler
{
    internal class MiniProfilerConnectionFactory : IConnectionFactory
    {
#pragma warning disable IDE1006 // Naming Styles
        private static readonly string ConnectionName = nameof(ProfiledDbConnection).ToLower();
#pragma warning restore IDE1006 // Naming Styles

        private readonly IConnectionFactory _factory;

        public Type DbConnectionType => typeof(ProfiledDbConnection);

        public MiniProfilerConnectionFactory(IConnectionFactory factory)
        {
            _factory = factory;
        }

        public DbConnection CreateConnection()
        {
            // Forward the call to the actual factory.
            var connection = _factory.CreateConnection();

            return new ProfiledDbConnection(connection, new CurrentDbProfiler(() => StackExchange.Profiling.MiniProfiler.Current));
        }
    }
}
