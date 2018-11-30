using System;
using System.Data;
using System.Threading.Tasks;
using OrchardCore.Data.Abstractions;

namespace OrchardCore.Data
{
    public class DbConnectionAccessor : IDbConnectionAccessor
    {
        private readonly YesSql.ISession _session;

        public DbConnectionAccessor(YesSql.ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<IDbConnection> GetConnectionAsync()
        {
            var transaction = await _session.DemandAsync();

            return transaction.Connection;
        }
    }
}
