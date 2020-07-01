using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

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
                    return default;
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

        private byte[] Serialize(object value)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, Formatting.None));

            // TODO: serialize as binary
            // return MessagePackSerializer.Typeless.Serialize(value, MessagePack.Resolvers.ContractlessStandardResolver.Options);
        }

        private T Deserialize<T>(byte[] data)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));

            // TODO: serialize as binary
            // return (T) MessagePackSerializer.Typeless.Deserialize(data, MessagePack.Resolvers.ContractlessStandardResolver.Options);
        }
    }
}
