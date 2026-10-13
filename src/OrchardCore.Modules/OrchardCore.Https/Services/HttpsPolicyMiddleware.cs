using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Https.Settings;

namespace OrchardCore.Https.Services;

/// <summary>
/// Applies the HTTPS redirection and the HTTP Strict Transport Security (HSTS) of the current
/// <see cref="HttpsSettings"/>, as the related middlewares copy the options they are built with.
/// </summary>
internal sealed class HttpsPolicyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<HttpsSettings> _httpsSettings;
    private readonly IOptionsMonitor<HttpsRedirectionOptions> _httpsRedirectionOptions;
    private readonly IOptions<HstsOptions> _hstsOptions;
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServerAddressesFeature _serverAddressesFeature;
    private readonly bool _isProduction;

    private HttpsPolicy _policy;

    public HttpsPolicyMiddleware(
        RequestDelegate next,
        IOptionsMonitor<HttpsSettings> httpsSettings,
        IOptionsMonitor<HttpsRedirectionOptions> httpsRedirectionOptions,
        IOptions<HstsOptions> hstsOptions,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        IHostEnvironment hostEnvironment,
        IServerAddressesFeature serverAddressesFeature)
    {
        _next = next;
        _httpsSettings = httpsSettings;
        _httpsRedirectionOptions = httpsRedirectionOptions;
        _hstsOptions = hstsOptions;
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _serverAddressesFeature = serverAddressesFeature;
        _isProduction = hostEnvironment.IsProduction();
    }

    public Task Invoke(HttpContext context)
        => GetPipeline().Invoke(context);

    private RequestDelegate GetPipeline()
    {
        var settings = _httpsSettings.CurrentValue;
        var redirectionOptions = _httpsRedirectionOptions.CurrentValue;

        var policy = _policy;
        if (ReferenceEquals(policy?.Settings, settings) && ReferenceEquals(policy.RedirectionOptions, redirectionOptions))
        {
            return policy.Pipeline;
        }

        var pipeline = _next;

        if (settings.StrictTransportSecurityMode == HttpStrictTransportSecurityMode.Enabled ||
            (settings.StrictTransportSecurityMode == HttpStrictTransportSecurityMode.FromConfiguration && _isProduction))
        {
            pipeline = new HstsMiddleware(pipeline, _hstsOptions, _loggerFactory).Invoke;
        }

        // The redirection is applied before HSTS, as when the middlewares were added to the tenant pipeline.
        if (settings.RequireHttps)
        {
            var options = Microsoft.Extensions.Options.Options.Create(redirectionOptions);

            pipeline = (_serverAddressesFeature is null
                ? new HttpsRedirectionMiddleware(pipeline, options, _configuration, _loggerFactory)
                : new HttpsRedirectionMiddleware(pipeline, options, _configuration, _loggerFactory, _serverAddressesFeature)).Invoke;
        }

        _policy = new HttpsPolicy(settings, redirectionOptions, pipeline);

        return pipeline;
    }

    private sealed record HttpsPolicy(HttpsSettings Settings, HttpsRedirectionOptions RedirectionOptions, RequestDelegate Pipeline);
}
