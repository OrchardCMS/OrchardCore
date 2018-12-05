using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using OrchardCore.Data.Abstractions;
using YesSql;

namespace OrchardCore.Data
{
    public class DbConnectionAccessor : IDbConnectionAccessor, IDisposable
    {
        private readonly IStore _store;
        private DbConnection _connection;
        private bool _disposed = false;

        public DbConnectionAccessor(IStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }
      
        public async Task<IDbConnection> GetConnectionAsync()
        {          
            if (_connection == null)
            {
                _connection = _store.Configuration.ConnectionFactory.CreateConnection() as DbConnection;
            }

            if (_connection.State == ConnectionState.Closed)
            {
                await _connection?.OpenAsync();
            }        
          
            return _connection;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {                    
                    _connection.Dispose();
                }
            }
            _disposed = true;
        }
    }
}
