using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Modules;

namespace OrchardCore.Caching
{
    /// <summary>
    /// Wrapper to ensure the system stays available during Redis outages
    /// </summary>
    public class DistributedCacheCircuitBreaker : IDistributedCache
    {
        private readonly IDistributedCache _underlyingCache;
        private readonly IClock _clock;

        private static DateTime _retryAt;
        private static bool _cacheUnavailable = false;
        private static readonly object _cacheUnavailableLock = new();

        public DistributedCacheCircuitBreaker(IDistributedCache cache, IClock clock)
        {
            _underlyingCache = cache;
            _clock = clock;
        }

        public byte[] Get(string key)
        {
            CheckDistributedErrorRetry();

            if (_cacheUnavailable)
            {
                return null;
            }

            try
            {
                return _underlyingCache.Get(key);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                RedisCacheFailed();
                return null;
            }
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            CheckDistributedErrorRetry();

            if (_cacheUnavailable)
            {
                return null;
            }

            try
            {
                return await _underlyingCache.GetAsync(key, token);
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

            if (_cacheUnavailable)
            {
                return;
            }

            try
            {
                _underlyingCache.Refresh(key);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                RedisCacheFailed();
            }
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            CheckDistributedErrorRetry();

            if (_cacheUnavailable)
            {
                return;
            }

            try
            {
                await _underlyingCache.RefreshAsync(key, token);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                RedisCacheFailed();
            }
        }

        public void Remove(string key)
        {
            CheckDistributedErrorRetry();

            if (_cacheUnavailable)
            {
                return;
            }

            try
            {
                _underlyingCache.Remove(key);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                RedisCacheFailed();
            }
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            CheckDistributedErrorRetry();

            if (_cacheUnavailable)
            {
                return;
            }

            try
            {
                await _underlyingCache.RemoveAsync(key, token);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                RedisCacheFailed();
            }
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            CheckDistributedErrorRetry();

            if (_cacheUnavailable)
            {
                return;
            }

            try
            {
                _underlyingCache.Set(key, value, options);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                RedisCacheFailed();
            }
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            CheckDistributedErrorRetry();

            if (_cacheUnavailable)
            {
                return;
            }

            try
            {
                await _underlyingCache.SetAsync(key, value, options, token);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                RedisCacheFailed();
            }
        }

        private void CheckDistributedErrorRetry()
        {
            if (!_cacheUnavailable)
            {
                return;
            }

            lock (_cacheUnavailableLock)
            {
                // Recheck value as it may have been changed before we could lock
                if (_cacheUnavailable && _clock.UtcNow > _retryAt)
                {
                    _cacheUnavailable = false;
                }
            }
        }
        private void RedisCacheFailed()
        {
            if (_cacheUnavailable)
            {
                return;
            }

            lock (_cacheUnavailableLock)
            {
                if (_cacheUnavailable)
                {
                    return;
                }

                // TODO: move to configuration or should this be an increasing number like RedisLock?
                _retryAt = _clock.UtcNow.AddSeconds(15);
                _cacheUnavailable = true;
            }
        }
    }
}
