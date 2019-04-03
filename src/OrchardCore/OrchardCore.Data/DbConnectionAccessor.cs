using System;
using System.Data.Common;
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
      
        public DbConnection CreateConnection()
        {          
            return _store.Configuration.ConnectionFactory.CreateConnection();
        }
    }
}
