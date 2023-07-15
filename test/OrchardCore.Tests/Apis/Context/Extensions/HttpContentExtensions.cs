namespace OrchardCore.Tests.Apis.Context
{
    internal static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content, JsonConverter jsonConverter)
        {
            using var stream = await content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            var ser = new JsonSerializer();
            ser.Converters.Insert(0, jsonConverter);
            return ser.Deserialize<T>(jsonReader);
        }

        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            using var data = await content.ReadAsStreamAsync();
            return data.ReadAs<T>();
        }

        public static T ReadAs<T>(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            var jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new StringEnumConverter() }
            };

            return jsonSerializer.Deserialize<T>(jsonReader);
        }
    }
}
