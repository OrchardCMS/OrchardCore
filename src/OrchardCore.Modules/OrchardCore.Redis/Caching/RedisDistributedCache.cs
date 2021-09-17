using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Modules;

namespace OrchardCore.Redis.Caching
{
    /// <summary>
    /// Wrapper to ensure the system stays available during Redis outages
    /// </summary>
    public class RedisDistributedCache : OrchardCore.Caching.Distributed.IDistributedCache
    {
        private readonly IDistributedCache _underlyingCache;
        private readonly IClock _clock;

        private static DateTime _retryAt;
        private static bool _redisUnavailable = false;
        private static object _redisUnavailableLock = new object();

        public RedisDistributedCache(Microsoft.Extensions.Caching.Distributed.IDistributedCache cache, IClock clock)
        {
            _underlyingCache = cache;
            _clock = clock;
        }

        public byte[] Get(string key)
        {
            CheckDistributedErrorRetry();

            if(_redisUnavailable)
                return Array.Empty<byte>();

            try {
                return _underlyingCache.Get(key);
            }
            catch (Exception)
            {
                RedisCacheFailed();
                return Array.Empty<byte>();
            }
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            CheckDistributedErrorRetry();

            if(_redisUnavailable)
                return Task.FromResult(Array.Empty<byte>());
            
            try {
                return _underlyingCache.GetAsync(key, token);
            } catch (Exception) {
                RedisCacheFailed();

                return Task.FromResult(Array.Empty<byte>());
            }
        }

        public async Task<string> GetStringAsync(string key)
        {
            CheckDistributedErrorRetry();

            if(_redisUnavailable)
                return null;
            
            try {
                return await _underlyingCache.GetStringAsync(key);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                RedisCacheFailed();

                return null;
            }
        }

        public void Refresh(string key)
        {
            CheckDistributedErrorRetry();

            if(_redisUnavailable)
                return;

            try {
                _underlyingCache.Refresh(key);
            }
            catch (Exception)
            {
                RedisCacheFailed();
            }
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            CheckDistributedErrorRetry();

            if(_redisUnavailable)
                return Task.CompletedTask;

            try {
                return _underlyingCache.RefreshAsync(key, token);
            } catch (Exception) {
                RedisCacheFailed();

                return Task.CompletedTask;
            }
        }

        public void Remove(string key)
        {
            CheckDistributedErrorRetry();

            if(_redisUnavailable)
                return;

            try {
                _underlyingCache.Remove(key);
            } catch (Exception) {
                RedisCacheFailed();
            }
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            CheckDistributedErrorRetry();

            if(_redisUnavailable)
                return Task.CompletedTask;

            try {
                return _underlyingCache.RemoveAsync(key, token);
            } catch (Exception) {
                RedisCacheFailed();

                return Task.CompletedTask;
            }
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            CheckDistributedErrorRetry();

            if(_redisUnavailable)
                return;

            try
            {
                _underlyingCache.Set(key, value, options);
            } catch (Exception) {
                RedisCacheFailed();
            }
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            CheckDistributedErrorRetry();

            if(_redisUnavailable)
                return Task.CompletedTask;

            try
            {
                return _underlyingCache.SetAsync(key, value, options, token);
            } catch (Exception) {
                RedisCacheFailed();

                return Task.CompletedTask;
            }
        }

        public Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options)
        {
            CheckDistributedErrorRetry();

            if(_redisUnavailable)
                return Task.CompletedTask;

            try
            {
                return _underlyingCache.SetStringAsync(key, value, options);
            } catch (Exception) {
                RedisCacheFailed();

                return Task.CompletedTask;
            }
        }

        private void CheckDistributedErrorRetry()
        {
            if(!_redisUnavailable)
                return;

            lock(_redisUnavailableLock)
            {
                // Recheck value as it may have been changed before we could lock
                if(_redisUnavailable && _clock.UtcNow > _retryAt)
                {
                    _redisUnavailable = false;
                }
            }
        }
        private void RedisCacheFailed()
        {
            if(_redisUnavailable)
                return;

            lock(_redisUnavailableLock)
            {
                if(_redisUnavailable)
                    return;

                // TODO: move to configuration or should this be an increasing number like RedisLock?
                _retryAt = _clock.UtcNow.AddSeconds(15);
                _redisUnavailable = true;
            }
        }
    }
}
