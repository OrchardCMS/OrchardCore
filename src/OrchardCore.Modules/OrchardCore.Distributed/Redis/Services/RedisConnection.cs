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
        private bool _initialized;
        private IDatabase _database;
        private readonly string _tenantName;
        private readonly IOptions<RedisOptions> _redisOptionsAccessor;

        private volatile ConnectionMultiplexer _connection;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public RedisConnection(ShellSettings shellSettings, IOptions<RedisOptions> redisOptionsAccessor, ILogger<RedisConnection> logger)
        {
            _tenantName = shellSettings.Name;
            _redisOptionsAccessor = redisOptionsAccessor;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public async Task<IDatabase> GetDatabaseAsync()
        {
            await ConnectAsync();
            return _database;
        }

        private async Task ConnectAsync()
        {
            if (_initialized)
            {
                return;
            }

            await _connectionLock.WaitAsync();
            try
            {
                if (!_initialized)
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync(_redisOptionsAccessor.Value.ConfigurationOptions);
                    _database = _connection.GetDatabase();
                    _initialized = true;
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