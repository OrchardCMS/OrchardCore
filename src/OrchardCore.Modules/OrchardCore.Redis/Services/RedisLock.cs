using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using StackExchange.Redis;

namespace OrchardCore.Redis.Services
{
    /// <summary>
    /// This component is a tenant singleton which allows to acquire distributed named locks for a given tenant.
    /// </summary>
    public class RedisLock : IDistributedLock
    {
        private readonly IRedisService _redis;
        private readonly ILogger _logger;
        private readonly string _hostName;
        private readonly string _prefix;

        public RedisLock(IRedisService redis, ShellSettings shellSettings, ILogger<RedisLock> logger)
        {
            _redis = redis;
            _hostName = Dns.GetHostName() + ':' + System.Environment.ProcessId;
            _prefix = redis.InstancePrefix + shellSettings.Name + ':';
            _logger = logger;
        }

        /// <summary>
        /// Waits indefinitely until acquiring a named lock with a given expiration for the current tenant.
        /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
        /// </summary>
        public async Task<ILocker> AcquireLockAsync(string key, TimeSpan? expiration = null)
        {
            return (await TryAcquireLockAsync(key, TimeSpan.MaxValue, expiration)).locker;
        }

        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
        /// </summary>
        public async Task<(ILocker locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan timeout, TimeSpan? expiration = null)
        {
            using (var cts = new CancellationTokenSource(timeout != TimeSpan.MaxValue ? timeout : Timeout.InfiniteTimeSpan))
            {
                var retries = 0.0;

                while (!cts.IsCancellationRequested)
                {
                    var locked = await LockAsync(key, expiration ?? TimeSpan.MaxValue);

                    if (locked)
                    {
                        return (new Locker(this, key), locked);
                    }

                    try
                    {
                        await Task.Delay(GetDelay(++retries), cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("Timeout elapsed before acquiring the named lock '{LockName}' after the given timeout of '{Timeout}'.",
                                _prefix + key, timeout.ToString());
                        }
                    }
                }
            }

            return (null, false);
        }

        public async Task<bool> IsLockAcquiredAsync(string key)
        {
            if (_redis.Database == null)
            {
                await _redis.ConnectAsync();
                if (_redis.Database == null)
                {
                    _logger.LogError("Fails to check whether the named lock '{LockName}' is already acquired.", _prefix + key);
                    return false;
                }
            }

            try
            {
                return (await _redis.Database.LockQueryAsync(_prefix + key)).HasValue;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Fails to check whether the named lock '{LockName}' is already acquired.", _prefix + key);
            }

            return false;
        }

        private async Task<bool> LockAsync(string key, TimeSpan expiry)
        {
            if (_redis.Database == null)
            {
                await _redis.ConnectAsync();
                if (_redis.Database == null)
                {
                    _logger.LogError("Fails to acquire the named lock '{LockName}'.", _prefix + key);
                    return false;
                }
            }

            try
            {
                return await _redis.Database.LockTakeAsync(_prefix + key, _hostName, expiry);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Fails to acquire the named lock '{LockName}'.", _prefix + key);
            }

            return false;
        }

        private async ValueTask ReleaseAsync(string key)
        {
            try
            {
                await _redis.Database.LockReleaseAsync(_prefix + key, _hostName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Fails to release the named lock '{LockName}'.", _prefix + key);
            }
        }

        private void Release(string key)
        {
            try
            {
                _redis.Database.LockRelease(_prefix + key, _hostName, CommandFlags.FireAndForget);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Fails to release the named lock '{LockName}'.", _prefix + key);
            }
        }

        private class Locker : ILocker
        {
            private readonly RedisLock _lock;
            private readonly string _key;
            private bool _disposed;

            public Locker(RedisLock redislock, string key)
            {
                _lock = redislock;
                _key = key;
            }

            public ValueTask DisposeAsync()
            {
                if (_disposed)
                {
                    return default;
                }

                _disposed = true;

                return _lock.ReleaseAsync(_key);
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                _lock.Release(_key);
            }
        }

        private static readonly double _baseDelay = 100;
        private static readonly double _maxDelay = 10000;

        private static TimeSpan GetDelay(double retries)
        {
            var delay = _baseDelay
                * (1.0 + ((Math.Pow(1.8, retries - 1.0) - 1.0)
                    * (0.6 + new Random().NextDouble() * 0.4)));

            return TimeSpan.FromMilliseconds(Math.Min(delay, _maxDelay));

            // 2 examples with 10 retries
            // --------------------------
            // 100     100 (start from base)
            // 164     171
            // 256     312
            // 401     519
            // 754     766
            // 1327    1562
            // 2950    3257
            // 4596    4966
            // 7215    8667
            // 10000   10000 (max reached)
        }
    }
}
