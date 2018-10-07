using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Distributed.Redis.Options;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Redis.Services
{
    public class RedisConnection : IRedisConnection, IDisposable
    {
        private readonly string _tenantName;
        private readonly IOptions<RedisOptions> _options;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1);
        private ConnectionMultiplexer _connection;
        private IDatabase _database;
        private bool _initialized;

        public RedisConnection(ShellSettings shellSettings, IOptions<RedisOptions> options, ILogger<RedisConnection> logger)
        {
            _tenantName = shellSettings.Name;
            _options = options;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public async Task<IDatabase> GetDatabaseAsync()
        {
            if (!_initialized)
            {
                await ConnectAsync();
            }

            return _database;
        }

        private async Task ConnectAsync()
        {
            await _connectionLock.WaitAsync();

            try
            {
                if (!_initialized)
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync(_options.Value.ConfigurationOptions);
                    _database = _connection.GetDatabase();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "'{TenantName}' is unable to connect to Redis.", _tenantName);
            }
            finally
            {
                _initialized = true;
                _connectionLock.Release();
            }
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
            }
        }
    }
}