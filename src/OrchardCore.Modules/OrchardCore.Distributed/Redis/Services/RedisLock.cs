using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Distributed.Redis.Services
{
    public class RedisLock : IDistributedLock
    {
        private readonly string _hostName;
        private readonly string _lockKeyPrefix;
        private readonly IRedisConnection _connection;

        public RedisLock(ShellSettings shellSettings, IRedisConnection connection, ILogger<RedisLock> logger)
        {
            _hostName = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id;
            _lockKeyPrefix = shellSettings.Name + ":";
            _connection = connection;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public async Task<bool> LockAsync(string key, TimeSpan expiry)
        {
            var database = await _connection.GetDatabaseAsync();

            if (database?.Multiplexer.IsConnected ?? false)
            {
                return await database.LockTakeAsync(_lockKeyPrefix + key, _hostName, expiry);
            }

            return false;
        }

        public async Task<bool> ReleaseAsync(string key)
        {
            var database = await _connection.GetDatabaseAsync();

            if (database?.Multiplexer.IsConnected ?? false)
            {
                return await database.LockReleaseAsync(_lockKeyPrefix + key, _hostName);
            }

            return false;
        }
    }
}