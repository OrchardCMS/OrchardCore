using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            using var data = await content.ReadAsStreamAsync();
            return await data.ReadAsAsync<T>();
        }

        public static ValueTask<T> ReadAsAsync<T>(this Stream stream) =>
            JsonSerializer.DeserializeAsync<T>(stream, JOptions.Default);
    }
}
