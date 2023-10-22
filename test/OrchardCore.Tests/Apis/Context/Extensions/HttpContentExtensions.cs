namespace OrchardCore.Tests.Apis.Context
{
    internal static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content, JsonConverter jsonConverter)
        {
            await using var stream = await content.ReadAsStreamAsync();

            var options = CreateOptions();

            if (jsonConverter != null)
            {
                options.Converters.Insert(0, jsonConverter);
            }

            return await JsonSerializer.DeserializeAsync<T>(stream, options);
        }

        public static Task<T> ReadAsAsync<T>(this HttpContent content) =>
            content.ReadAsAsync<T>(jsonConverter: null);

        private static JsonSerializerOptions CreateOptions()
        {
            return new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() },
            };
        }
    }
}
