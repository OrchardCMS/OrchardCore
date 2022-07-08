using System;
using System.Buffers;
using Newtonsoft.Json;
using OrchardCore.Abstractions.Pooling;
using YesSql;

namespace OrchardCore.Data
{
    /// <summary>
    /// Custom YesSql content serializer which forwards to generic pooling JSON serializer with custom settings.
    /// </summary>
    internal sealed class PoolingJsonContentSerializer : IContentSerializer
    {
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            CheckAdditionalContent = false
        };

        private readonly PoolingJsonSerializer _inner;

        public PoolingJsonContentSerializer(ArrayPool<char> arrayPool)
        {
            _inner = new PoolingJsonSerializer(arrayPool, _jsonSettings);
        }

        public object Deserialize(string content, Type type) => _inner.Deserialize(content, type);

        public dynamic DeserializeDynamic(string content) => _inner.Deserialize<dynamic>(content);

        public string Serialize(object item) => _inner.Serialize(item);
    }
}
