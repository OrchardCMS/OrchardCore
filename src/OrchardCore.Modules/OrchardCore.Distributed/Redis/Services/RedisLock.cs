using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Redis.Services
{
    /// <summary>
    /// This component is a tenant singleton which allows to acquire named locks for a given tenant.
    /// This is a distributed version where locks are auto released after provided expiration times.
    /// </summary>
    public class RedisLock : ILock
    {
        private readonly string _hostName;
        private readonly string _lockKeyPrefix;
        private readonly IRedisConnection _connection;

        public RedisLock(ShellSettings shellSettings, IRedisConnection connection)
        {
            _hostName = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id;
            _lockKeyPrefix = shellSettings.Name + ":";
            _connection = connection;
        }

        /// <summary>
		/// Tries to immediately acquire a named lock with a given expiration time within the current tenant.
        /// </summary>
        public async Task<(IDisposable locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan? expiration = null)
        {
            return (new Locker(this, key), await LockAsync(key, expiration ?? TimeSpan.FromSeconds(1)));
        }

        private async Task<bool> LockAsync(string key, TimeSpan expiry)
        {
            var database = await _connection.GetDatabaseAsync();

            if (database?.Multiplexer.IsConnected ?? false)
            {
                return await database.LockTakeAsync(_lockKeyPrefix + key, _hostName, expiry);
            }

            return false;
        }

        public void Release(string key)
        {
            var database = _connection.GetDatabaseAsync().Result;

            if (database?.Multiplexer.IsConnected ?? false)
            {
                database.LockRelease(_lockKeyPrefix + key, _hostName, CommandFlags.FireAndForget);
            }
        }

        private class Locker : IDisposable
        {
            private readonly RedisLock _lock;
            private readonly string _key;

            public Locker(RedisLock rlock, string key)
            {
                _lock = rlock;
                _key = key;
            }

            public void Dispose()
            {
                _lock.Release(_key);
            }
        }
    }
}