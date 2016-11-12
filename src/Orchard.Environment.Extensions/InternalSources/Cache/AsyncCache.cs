using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cache
{
    public class AsyncCache
    {
        private Dictionary<string, CacheItem> dictionary = new Dictionary<string, CacheItem>();
        private ConcurrentDictionary<string, AsyncLock> locks = new ConcurrentDictionary<string, AsyncLock>();
        private Func<DateTimeOffset> timeProvider;
        private TimeSpan keyLifeTime;

        public AsyncCache() : this(TimeSpan.FromMinutes(5)) { }
        public AsyncCache(TimeSpan keyLifeTime) : this(() => DateTimeOffset.UtcNow, keyLifeTime) { }
        public AsyncCache(Func<DateTimeOffset> timeProvider, TimeSpan keyLifeTime)
        {
            this.timeProvider = timeProvider;
            this.keyLifeTime = keyLifeTime;
        }

        public async Task<T> Get<T>(string key, Func<string, Task<T>> dataSource)
        {
            using (var releaser = await locks.GetOrAdd(key, s => new AsyncLock()).LockAsync())
            {
                var currentTime = timeProvider();

                if (!dictionary.ContainsKey(key) || currentTime >= dictionary[key].Expiration)
                {
                    dictionary[key] = new CacheItem
                    {
                        Item = await dataSource(key),
                        Expiration = currentTime.Add(keyLifeTime)
                    };
                }
                return (T)dictionary[key].Item;
            }
        }

        public void Clear(string key)
        {
            dictionary.Remove(key);
        }

        #region Helper classes

        public class CacheItem
        {
            public object Item { get; set; }
            public DateTimeOffset Expiration { get; set; }
        }

        #endregion
    }
}