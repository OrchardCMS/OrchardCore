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
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public DefaultJsonContentSerializer(IEnumerable<IJsonTypeInfoResolver> typeInfoResolvers)
        {
            _options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            foreach (var resolver in typeInfoResolvers)
            {
                _options.TypeInfoResolverChain.Add(resolver);
            }
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

        public dynamic DeserializeDynamic(string content) => JsonSerializer.Deserialize<dynamic>(content);

        public string Serialize(object item) => JsonSerializer.Serialize(item, _options);
    }
}
