using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace YesSql.Serialization
{
    public class DefaultJsonContentSerializer : IContentSerializer
    {
        private readonly JsonSerializerOptions _options;

        public DefaultJsonContentSerializer()
        {
            _options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            _options.Converters.Add(JsonDynamicConverter.Instance);
        }

        public DefaultJsonContentSerializer(IEnumerable<IJsonTypeInfoResolver> typeInfoResolvers)
        {
            _options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            foreach (var resolver in typeInfoResolvers)
            {
                _options.TypeInfoResolverChain.Add(resolver);
            }

            _options.Converters.Add(JsonDynamicConverter.Instance);
        }

        public DefaultJsonContentSerializer(JsonSerializerOptions options) => _options = options;

        public object Deserialize(string content, Type type)
        {
            try
            {
                return JsonSerializer.Deserialize(content, type, _options);
            }
            catch
            {
                return default;
            }
        }

        public dynamic DeserializeDynamic(string content) => JsonSerializer.Deserialize<dynamic>(content, _options);

        public string Serialize(object item) => JsonSerializer.Serialize(item, _options);
    }
}
