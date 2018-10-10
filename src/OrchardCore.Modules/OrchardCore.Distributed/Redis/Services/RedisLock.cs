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
        private readonly string _prefix;
        private readonly IRedisClient _redis;

        public RedisLock(ShellSettings shellSettings, IRedisClient redis)
        {
            _hostName = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id;
            _prefix = shellSettings.Name + ":";
            _redis = redis;
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
            await _redis.ConnectAsync();

            if (_redis.IsConnected)
            {
                return await _redis.Database.LockTakeAsync(_prefix + key, _hostName, expiry);
            }

            return false;
        }

        public void Release(string key)
        {
            if (_redis.IsConnected)
            {
                _redis.Database.LockRelease(_prefix + key, _hostName, CommandFlags.FireAndForget);
            }
        }

        private class Locker : IDisposable
        {
            private readonly RedisLock _lock;
            private readonly string _key;

            public Locker(RedisLock lock_, string key)
            {
                _lock = lock_;
                _key = key;
            }

            public void Dispose()
            {
                _lock.Release(_key);
            }
        }
    }
}