using System;
using System.Text.Json;
using System.Text.Json.Serialization;

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

            // _options.Converters.Add(UtcDateTimeJsonConverter.Instance);
            // _options.Converters.Add(DynamicJsonConverter.Instance);
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
