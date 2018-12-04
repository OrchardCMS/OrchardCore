using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using OrchardCore.Data.Abstractions;
using YesSql;

namespace OrchardCore.Data
{
    public class DbConnectionAccessor : IDbConnectionAccessor
    {
        private readonly IStore _store;

        public DbConnectionAccessor(IStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<IDbConnection> GetConnectionAsync()
        {
            var connection = _store.Configuration.ConnectionFactory.CreateConnection() as DbConnection;
            await connection?.OpenAsync();
            return connection;
        }
    }
}
