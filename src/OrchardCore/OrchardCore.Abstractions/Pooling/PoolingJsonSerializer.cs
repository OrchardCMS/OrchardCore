using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace OrchardCore.Abstractions.Pooling
{
    /// <summary>
    /// Handles JSON.NET serialization utilizing pooled array buffers and string builders.
    /// </summary>
    internal sealed class PoolingJsonSerializer
    {
        private readonly JsonArrayPool<char> _arrayPool;
        private readonly JsonSerializerSettings _jsonSettings;

        public PoolingJsonSerializer(ArrayPool<char> arrayPool) : this(arrayPool, null)
        {
        }

        public PoolingJsonSerializer(ArrayPool<char> arrayPool, JsonSerializerSettings serializerSettings)
        {
            _arrayPool = new JsonArrayPool<char>(arrayPool ?? throw new ArgumentException("Array pool is required", nameof(arrayPool)));
            _jsonSettings = serializerSettings;
        }

        public T Deserialize<T>(string content) => (T) Deserialize(content, typeof(T));

        public object Deserialize(string content, Type type)
        {
            var jsonSerializer = JsonSerializer.CreateDefault(_jsonSettings);
            using var reader = new JsonTextReader(new StringReader(content))
            {
                ArrayPool = _arrayPool
            };
            return jsonSerializer.Deserialize(reader, type);
        }

        public string Serialize(object item)
        {
            var jsonSerializer = JsonSerializer.CreateDefault(_jsonSettings);
            using var sw = new ZStringWriter(CultureInfo.InvariantCulture);
            using var jsonWriter = new JsonTextWriter(sw)
            {
                ArrayPool = _arrayPool,
                Formatting = jsonSerializer.Formatting
            };
            jsonSerializer.Serialize(jsonWriter, item, null);
            return sw.ToString();
        }
    }
}
