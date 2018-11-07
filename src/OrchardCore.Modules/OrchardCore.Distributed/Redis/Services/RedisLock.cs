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
        /// Waits indefinitely until acquiring a named lock with a given expiration for the current tenant.
        /// </summary>
        public async Task<IDisposable> AcquireLockAsync(string key, TimeSpan? expiration = null)
        {
            return (await TryAcquireLockAsync(key, TimeSpan.MaxValue, expiration)).locker;
        }

        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// </summary>
        public async Task<(IDisposable locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan timeout, TimeSpan? expiration = null)
        {
            long maxCount = (long)(timeout.TotalMilliseconds / 1000);

            long count = 0;
            while (true)
            {
                var locked = await LockAsync(key, expiration ?? TimeSpan.FromSeconds(1));

                if (locked)
                {
                    return (new Locker(this, key), locked);
                }

                if (count++ > maxCount)
                {
                    return (null, locked);
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
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