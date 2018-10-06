using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Distributed.Redis.Options;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Redis.Services
{
    public class RedisLock : IDistributedLock, IDisposable
    {
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private volatile ConnectionMultiplexer _connection;

        private bool _initialized;
        private IDatabase _database;
        private readonly string _hostName;
        private readonly string _tenantName;
        private readonly string _lockKeyPrefix;

        private readonly IOptions<RedisOptions> _redisOptionsAccessor;

        public RedisLock(ShellSettings shellSettings, IOptions<RedisOptions> redisOptionsAccessor, ILogger<RedisLock> logger)
        {
            _hostName = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id;

            _tenantName = shellSettings.Name;
            _lockKeyPrefix = _tenantName + ":";

            _redisOptionsAccessor = redisOptionsAccessor;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public async Task<bool> LockTakeAsync(string key, TimeSpan expiry)
        {
            await ConnectAsync();

            if (_database?.Multiplexer.IsConnected ?? false)
            {
                if (_database.LockQueryAsync(_lockKeyPrefix + key).ToString() == _hostName)
                {
                    return await _database.LockExtendAsync(_lockKeyPrefix + key, _hostName, expiry);
                }

                return await _database.LockTakeAsync(_lockKeyPrefix + key, _hostName, expiry);
            }

            return false;
        }

        public async Task<bool> LockReleaseAsync(string key)
        {
            await ConnectAsync();

            if (_database?.Multiplexer.IsConnected ?? false)
            {
                return await _database.LockReleaseAsync(_lockKeyPrefix + key, _hostName);
            }

            return false;
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