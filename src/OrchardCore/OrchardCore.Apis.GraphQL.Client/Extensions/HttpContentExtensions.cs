using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrchardCore.Apis.GraphQL.Client
{
    internal static class HttpContentExtensions
    {
        private static readonly JsonSerializerOptions _options = new()
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

        public static ValueTask<T> ReadAsAsync<T>(this Stream stream) => JsonSerializer.DeserializeAsync<T>(stream, _options);
    }
}
