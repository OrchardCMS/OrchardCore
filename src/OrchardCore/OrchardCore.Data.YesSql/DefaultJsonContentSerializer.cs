using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using OrchardCore.Json.Serialization;

namespace YesSql.Serialization
{
    public class DefaultJsonContentSerializer : IContentSerializer
    {
        private readonly JsonSerializerOptions _options;

        public DefaultJsonContentSerializer()
        {
            _options = new(JOptions.Base);
            _options.Converters.Add(System.Text.Json.Serialization.DynamicJsonConverter.Instance);
            _options.Converters.Add(PathStringJsonConverter.Instance);
        }

        public DefaultJsonContentSerializer(IEnumerable<IJsonTypeInfoResolver> typeInfoResolvers)
        {
            _options = new(JOptions.Base);
            foreach (var resolver in typeInfoResolvers)
            {
                _options.TypeInfoResolverChain.Add(resolver);
            }

            _options.Converters.Add(System.Text.Json.Serialization.DynamicJsonConverter.Instance);
            _options.Converters.Add(PathStringJsonConverter.Instance);
        }

        public DefaultJsonContentSerializer(JsonSerializerOptions options) => _options = options;

        public object Deserialize(string content, Type type)
            => JsonSerializer.Deserialize(content, type, _options);

        public dynamic DeserializeDynamic(string content) => JsonSerializer.Deserialize<dynamic>(content, _options);

        public string Serialize(object item) => JsonSerializer.Serialize(item, _options);
    }
}
