using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Infrastructure.Cache
{
    public class ScopedDistributedCache : IScopedDistributedCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly Dictionary<string, object> _scopedCache = new Dictionary<string, object>();

        public ScopedDistributedCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (!_scopedCache.TryGetValue(key, out var value))
            {
                var data = await _distributedCache.GetAsync(key);

                if (data == null)
                {
                    return default(T);
                }

                value = Deserialize<T>(data);

                _scopedCache[key] = value;
            }

            return (T)value;
        }

        public async Task SetAsync<T>(string key, T value)
        {
            var data = Serialize(value);

            await _distributedCache.SetAsync(key, data);

            _scopedCache[key] = value;
        }

        private byte[] Serialize<T>(T value)
        {
            return MessagePackSerializer.Serialize(value, ContractlessStandardResolver.Options);
        }

        private T Deserialize<T>(byte[] data)
        {
            return MessagePackSerializer.Deserialize<T>(data, ContractlessStandardResolver.Options);
        }
    }
}
