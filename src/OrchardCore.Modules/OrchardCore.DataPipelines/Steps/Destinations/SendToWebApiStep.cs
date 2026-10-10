using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Services;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Sends each file it reads to a web API, in one HTTP request per file. A response with a status code other than 2xx
/// fails the step.
/// </summary>
public sealed partial class SendToWebApiStep : DataPipelineStepType<SendToWebApiStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "SendToWebApi";

    /// <summary>
    /// The name of the <see cref="HttpClient"/> the step sends requests with. Configure it with
    /// <c>services.AddHttpClient(SendToWebApiStep.HttpClientName)</c>, such as to add a proxy.
    /// </summary>
    public const string HttpClientName = "DataPipelines";

    /// <summary>
    /// The number of characters of the body of a failed response that the error shows.
    /// </summary>
    public const int MaxErrorBodyLength = 500;

    private static readonly HashSet<string> _reservedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Host",
        "Content-Length",
        "Transfer-Encoding",
        "Connection",
    };

    private static readonly HashSet<string> _credentialHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Proxy-Authorization",
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DataPipelineSecrets _secrets;
    private readonly IStringLocalizer S;

    public SendToWebApiStep(
        IHttpClientFactory httpClientFactory,
        DataPipelineSecrets secrets,
        IStringLocalizer<SendToWebApiStep> localizer)
    {
        _httpClientFactory = httpClientFactory;
        _secrets = secrets;
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Send to a web API"];

    public override LocalizedString Description => S["Sends each file to a web API, in an HTTP request."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Destination;

    public override string Icon => "fa-solid fa-cloud-arrow-up";

    /// <summary>
    /// Describes a URL without its credentials, query string and fragment, which may hold secrets.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The description, or <see langword="null"/> when the URL is not absolute.</returns>
    public static string GetDisplayUrl(string url)
        => Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)
            ? uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped)
            : null;

    /// <summary>
    /// Tells whether a text is a valid HTTP header name.
    /// </summary>
    /// <param name="name">The text.</param>
    /// <returns><see langword="true"/> when it is a valid header name.</returns>
    public static bool IsValidHeaderName(string name)
        => !string.IsNullOrEmpty(name) && HeaderNamePattern().IsMatch(name);

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);

        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            context.AddError(S["Enter the URL of the web API."]);
        }
        else if (!Uri.TryCreate(settings.Url.Trim(), UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            context.AddError(S["The URL of the web API must be an absolute http or https URL."]);
        }
        else if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            context.AddError(S["The URL can't hold credentials: set them in the authentication settings."]);
        }
        else if (uri.Scheme == Uri.UriSchemeHttp && settings.Authentication != DataPipelineHttpAuthentication.None)
        {
            context.AddWarning(S["The URL isn't https: the credentials and the files travel in clear text."]);
        }

        if (settings.BodyMode == DataPipelineHttpBodyMode.Multipart && string.IsNullOrWhiteSpace(settings.FormFieldName))
        {
            context.AddError(S["Enter the name of the form field that holds the file."]);
        }

        foreach (var header in settings.Headers ?? [])
        {
            var name = header?.Name?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            if (!IsValidHeaderName(name))
            {
                context.AddError(S["'{0}' isn't a valid header name.", name]);
            }
            else if (_credentialHeaders.Contains(name))
            {
                context.AddError(S["Headers are not secret: set the credentials with the authentication settings, not with the '{0}' header.", name]);
            }
            else if (_reservedHeaders.Contains(name))
            {
                context.AddError(S["The '{0}' header is set by the step.", name]);
            }
            else if (header.Value?.IndexOfAny(['\r', '\n']) >= 0)
            {
                context.AddError(S["The value of the '{0}' header can't hold line breaks.", name]);
            }
        }

        switch (settings.Authentication)
        {
            case DataPipelineHttpAuthentication.Basic:
                if (string.IsNullOrWhiteSpace(settings.Username))
                {
                    context.AddError(S["Enter the user name of the basic authentication."]);
                }

                CheckSecret(context, settings.ProtectedPassword, required: false);
                break;

            case DataPipelineHttpAuthentication.Bearer:
                CheckSecret(context, settings.ProtectedToken, required: true);
                break;

            case DataPipelineHttpAuthentication.ApiKey:
                if (!IsValidHeaderName(settings.ApiKeyHeaderName?.Trim()))
                {
                    context.AddError(S["Enter the name of the header that holds the API key."]);
                }

                CheckSecret(context, settings.ProtectedApiKey, required: true);
                break;
        }

        if (settings.TimeoutSeconds < 1)
        {
            context.AddError(S["The timeout must be at least one second."]);
        }

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var url = new Uri(settings.Url.Trim(), UriKind.Absolute);
        var displayUrl = GetDisplayUrl(settings.Url);
        var method = settings.Method == DataPipelineHttpMethod.Put ? HttpMethod.Put : HttpMethod.Post;
        var timeout = TimeSpan.FromSeconds(Math.Max(1, settings.TimeoutSeconds));
        var authorization = GetAuthorization(settings, out var secret);

        using var client = _httpClientFactory.CreateClient(HttpClientName);
        client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;

        await foreach (var file in context.GetInput().ReadFilesAsync(context.CancellationToken))
        {
            await using var stream = file.OpenRead();
            using var request = new HttpRequestMessage(method, url)
            {
                Content = CreateContent(settings, file, stream),
            };

            foreach (var header in settings.Headers ?? [])
            {
                var name = header?.Name?.Trim();

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (!request.Headers.TryAddWithoutValidation(name, header.Value ?? string.Empty))
                {
                    request.Content.Headers.Remove(name);
                    request.Content.Headers.TryAddWithoutValidation(name, header.Value ?? string.Empty);
                }
            }

            if (authorization is { } value)
            {
                request.Headers.Remove(value.Name);
                request.Headers.TryAddWithoutValidation(value.Name, value.Value);
            }

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            timeoutSource.CancelAfter(timeout);

            HttpResponseMessage response;

            try
            {
                response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutSource.Token);
            }
            catch (OperationCanceledException) when (!context.CancellationToken.IsCancellationRequested)
            {
                throw new DataPipelineDestinationException(S["The web API at {0} didn't answer within {1} seconds when sending '{2}'.", displayUrl, (int)timeout.TotalSeconds, file.FileName]);
            }
            catch (HttpRequestException ex)
            {
                throw new DataPipelineDestinationException(
                    S["Could not send '{0}' to {1}: {2}", file.FileName, displayUrl, DataPipelineRemotePaths.Redact(ex.Message, secret, url.Query)],
                    ex);
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    var body = await ReadStartAsync(response, timeoutSource.Token);

                    throw new DataPipelineDestinationException(
                        S["The web API at {0} answered {1} ({2}) to '{3}': {4}", displayUrl, (int)response.StatusCode, response.ReasonPhrase, file.FileName, DataPipelineRemotePaths.Redact(body, secret)]);
                }

                context.AddDelivery(S["Sent '{0}' to {1} ({2})", file.FileName, displayUrl, (int)response.StatusCode]);
            }
        }
    }

    private void CheckSecret(DataPipelineDescribeContext context, string protectedValue, bool required)
    {
        if (string.IsNullOrEmpty(protectedValue))
        {
            if (required)
            {
                context.AddError(S["Enter the secret of the authentication."]);
            }
        }
        else if (_secrets.Unprotect(protectedValue) is null)
        {
            context.AddError(S["The secret of the authentication can't be read on this site, such as after an import: enter it again."]);
        }
    }

    private (string Name, string Value)? GetAuthorization(SendToWebApiStepSettings settings, out string secret)
    {
        secret = null;

        switch (settings.Authentication)
        {
            case DataPipelineHttpAuthentication.Basic:
                secret = _secrets.Unprotect(settings.ProtectedPassword);
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.Username?.Trim()}:{secret}"));

                return ("Authorization", "Basic " + credentials);

            case DataPipelineHttpAuthentication.Bearer:
                secret = _secrets.Unprotect(settings.ProtectedToken);

                return ("Authorization", "Bearer " + secret);

            case DataPipelineHttpAuthentication.ApiKey:
                secret = _secrets.Unprotect(settings.ProtectedApiKey);

                return (settings.ApiKeyHeaderName.Trim(), secret);

            default:
                return null;
        }
    }

    private static HttpContent CreateContent(SendToWebApiStepSettings settings, DataPipelineFile file, Stream stream)
    {
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.TryParse(file.ContentType, out var contentType)
            ? contentType
            : new MediaTypeHeaderValue("application/octet-stream");

        if (settings.BodyMode == DataPipelineHttpBodyMode.Raw)
        {
            return fileContent;
        }

        return new MultipartFormDataContent
        {
            { fileContent, settings.FormFieldName.Trim(), file.FileName },
        };
    }

    private static async Task<string> ReadStartAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var buffer = new char[MaxErrorBodyLength];
            var length = 0;

            while (length < buffer.Length)
            {
                var read = await reader.ReadAsync(buffer.AsMemory(length), cancellationToken);

                if (read == 0)
                {
                    break;
                }

                length += read;
            }

            return new string(buffer, 0, length).Trim();
        }
        catch (Exception ex) when (ex is IOException or HttpRequestException)
        {
            return string.Empty;
        }
    }

    // The characters of a token, as defined by RFC 9110.
    [GeneratedRegex(@"^[!#$%&'*+\-.^_`|~0-9A-Za-z]+$")]
    private static partial Regex HeaderNamePattern();
}
