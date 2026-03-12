using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using OrchardCore.Infrastructure;

namespace OrchardCore.Apis.GraphQL.Client;

/// <summary>
/// The http request extensions.
/// </summary>
internal static class HttpRequestExtensions
{
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
    /// <param name="options">
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
        JsonSerializerOptions options = null)
    {
        var content = new StringContent(
            JConvert.SerializeObject(value, options),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

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
    /// <param name="options">
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
        JsonSerializerOptions options = null)
    {
        var content = new StringContent(
            JConvert.SerializeObject(value, options),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        return client.PutAsync(requestUri, content);
    }

    /// <summary>
    /// PostAsJsonAsync.
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
    /// <param name="options">
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
        JsonSerializerOptions options = null)
    {
        var content = new StringContent(
            JConvert.SerializeObject(value, options),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content,
        };

        request.Headers
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

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
            MediaTypeNames.Application.Json);

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content,
        };

        request.Headers
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

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
            MediaTypeNamesExtended.Application.JsonVendeorPrefix);


        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content,
        };

        request.Headers
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue(MediaTypeNamesExtended.Application.JsonVendeorPrefix));

        return client.SendAsync(request);
    }
}
