using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OrchardCore.Apis.GraphQL.Client
{
    /// <summary>
    /// The http request extensions.
    /// </summary>
    internal static class HttpRequestExtensions
    {
        private readonly static JsonSerializerSettings _jsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// The patch as json async.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="requestUri">
        /// The request uri.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="settings">
        /// The formatter.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(
            this HttpClient client,
            string requestUri,
            T value,
            JsonSerializerSettings settings = null)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(value, settings ?? _jsonSettings),
                Encoding.UTF8,
                "application/json");

            return HttpRequestExtensions.PatchAsync(client, requestUri, content);
        }

        /// <summary>
        /// The patch async.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="requestUri">
        /// The request uri.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task<HttpResponseMessage> PatchAsync(
            this HttpClient client,
            string requestUri,
            HttpContent content)
        {
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(client.BaseAddress + requestUri),
                Content = content
            };

            request.Headers.ExpectContinue = false;
            return client.SendAsync(request);
        }

        /// <summary>
        /// The put as json async.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="requestUri">
        /// The request uri.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="settings">
        /// The formatter.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task<HttpResponseMessage> PutAsJsonAsync<T>(
            this HttpClient client,
            string requestUri,
            T value,
            JsonSerializerSettings settings = null)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(value, settings ?? _jsonSettings),
                Encoding.UTF8,
                "application/json");

            return client.PutAsync(requestUri, content);
        }

        /// <summary>
        /// PostAsJsonAsync
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="requestUri">
        /// The request uri.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="settings">
        /// The formatter.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(
            this HttpClient client,
            string requestUri,
            T value,
            JsonSerializerSettings settings = null)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(value, settings ?? _jsonSettings),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content,
            };

            request.Headers
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client.SendAsync(request);
        }

        public static Task<HttpResponseMessage> PostJsonAsync(
            this HttpClient client,
            string requestUri,
            string json)
        {
            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content,
            };

            request.Headers
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client.SendAsync(request);
        }

        public static Task<HttpResponseMessage> PostJsonApiAsync(
            this HttpClient client,
            string requestUri,
            string json)
        {
            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/vnd.api+json");

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content,
            };

            request.Headers
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));

            return client.SendAsync(request);
        }
    }
}
