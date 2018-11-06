using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

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

        public RedisLock(ShellSettings shellSettings, IRedisClient redis, ILogger<RedisLock> logger)
        {
            _hostName = Dns.GetHostName() + ':' + Process.GetCurrentProcess().Id;
            _prefix = shellSettings.Name + ':';
            _redis = redis;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// Todo: timeout is not yet implemented.
        /// </summary>
        public async Task<(IDisposable locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan? timeout = null, TimeSpan? expiration = null)
        {
            return (new Locker(this, key), await LockAsync(key, expiration ?? TimeSpan.FromSeconds(1)));
        }

        private async Task<bool> LockAsync(string key, TimeSpan expiry)
        {
            await _redis.ConnectAsync();

            if (_redis.IsConnected)
            {
                try
                {
                    return await _redis.Database.LockTakeAsync(_prefix + key, _hostName, expiry);
                }

                catch (Exception e)
                {
                    Logger.LogError(e, "'Fails to acquire the named lock {LockName}'.", _prefix + key);
                }
            }

            return false;
        }

        public void Release(string key)
        {
            if (_redis.IsConnected)
            {
                try
                {
                    _redis.Database.LockRelease(_prefix + key, _hostName, CommandFlags.FireAndForget);
                }

                catch (Exception e)
                {
                    Logger.LogError(e, "'Fails to release the named lock {LockName}'.", _prefix + key);
                }
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