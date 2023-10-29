using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OrchardCore.Tests.Apis.Context
{
    internal static class HttpContentExtensions
    {
        private readonly static JsonSerializerOptions _jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNameCaseInsensitive = true,
        };

        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            using var data = await content.ReadAsStreamAsync();
            return await data.ReadAsAsync<T>();
        }

        public static ValueTask<T> ReadAsAsync<T>(this Stream stream) => JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
    }
}
