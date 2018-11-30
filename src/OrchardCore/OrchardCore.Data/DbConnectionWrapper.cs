using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Data
{
    public class DbConnectionWrapper : IDbConnectionWrapper
    {
        private readonly YesSql.ISession _session;

        public DbConnectionWrapper(YesSql.ISession session)
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
