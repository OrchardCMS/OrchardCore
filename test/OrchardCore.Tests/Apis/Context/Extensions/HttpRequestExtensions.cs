namespace OrchardCore.Tests.Apis.Context
{
    /// <summary>
    /// The http request extensions.
    /// </summary>
    internal static class HttpRequestExtensions
    {
        private static readonly JsonSerializerOptions _jsonSettings = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
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
        /// The serializer settings.
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
            JsonSerializerOptions settings = null)
        {
            var content = CreateContent(value, settings);

            return PatchAsync(client, requestUri, content);
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
                Content = content,
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
        /// The serializer settings.
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
            JsonSerializerOptions settings = null)
        {
            var content = CreateContent(value, settings);

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
        /// The serializer settings.
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
            JsonSerializerOptions settings = null)
        {
            var content = CreateContent(value, settings);

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
            var content = CreateContent(json, "application/json");

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
            var content = CreateContent(json, "application/vnd.api+json");

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content,
            };

            request.Headers
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));

            return client.SendAsync(request);
        }

        private static StringContent CreateContent(string json, string mediaType) =>
            new(json, Encoding.UTF8, mediaType);

        private static StringContent CreateContent<T>(T value, JsonSerializerOptions settings) =>
            CreateContent(JsonSerializer.Serialize(value, settings ?? _jsonSettings), "application/json");
    }
}
