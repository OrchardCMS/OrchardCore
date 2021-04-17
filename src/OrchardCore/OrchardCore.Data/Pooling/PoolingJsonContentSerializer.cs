using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using YesSql;

namespace OrchardCore.Data.Pooling
{
    internal sealed class PoolingJsonContentSerializer : IContentSerializer
    {
        private readonly ArrayPool<char> _arrayPool;

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            CheckAdditionalContent = false
        };

        public PoolingJsonContentSerializer(ArrayPool<char> arrayPool)
        {
            _arrayPool = arrayPool;
        }

        public object Deserialize(string content, Type type)
        {
            var jsonSerializer = JsonSerializer.CreateDefault(_jsonSettings);
            using var reader = new JsonTextReader(new StringReader(content))
            {
                ArrayPool = _arrayPool != null ? new JsonArrayPool<char>(_arrayPool) : null
            };
            return jsonSerializer.Deserialize(reader, type);
        }

        public dynamic DeserializeDynamic(string content)
        {
            return Deserialize<dynamic>(content);
        }

        // helps to resolve dynamic type
        private object Deserialize<T>(string content)
        {
            return Deserialize(content, typeof(T));
        }

        public string Serialize(object item)
        {
            var jsonSerializer = JsonSerializer.CreateDefault(_jsonSettings);
            using var pool = StringBuilderPool.GetInstance();
            var sw = new StringWriter(pool.Builder, CultureInfo.InvariantCulture);
            using var jsonWriter = new JsonTextWriter(sw)
            {
                ArrayPool = _arrayPool != null ? new JsonArrayPool<char>(_arrayPool) : null,
                Formatting = _jsonSettings.Formatting
            };
            jsonSerializer.Serialize(jsonWriter, item, null);
            return sw.ToString();
        }
    }
}
