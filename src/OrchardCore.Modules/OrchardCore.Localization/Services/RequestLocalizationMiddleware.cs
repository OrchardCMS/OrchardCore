using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Localization.Services;
/// <summary>
/// Enables automatic setting of the culture for <see cref="HttpRequest"/>s based on information
/// sent by the client in headers and logic provided by the application.
/// The middleware is inspired by <see href="https://github.com/dotnet/aspnetcore/blob/main/src/Middleware/Localization/src/RequestLocalizationMiddleware.cs"/>
/// and should have the same logic.
/// </summary>
public class RequestLocalizationMiddleware
{
    private const int MaxCultureFallbackDepth = 5;

    private readonly RequestDelegate _next;
    private readonly CultureOptions _cultureOptions;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new <see cref="RequestLocalizationMiddleware"/>.
    /// </summary>
    /// <param name="next">The <see cref="RequestDelegate"/> representing the next middleware in the pipeline.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used for logging.</param>
    /// <param name="cultureOptions">The <see cref="CultureOptions"/> used for logging.</param>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> used for logging.</param>
    /// <see cref="RequestLocalizationMiddleware"/>
    public RequestLocalizationMiddleware(
        RequestDelegate next,
        ILoggerFactory loggerFactory,
        IOptions<CultureOptions> cultureOptions,
        ILocalizationService localizationService)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _cultureOptions = cultureOptions.Value;
        _localizationService = localizationService;
        _logger = loggerFactory?.CreateLogger<RequestLocalizationMiddleware>() ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    /// Invokes the logic of the middleware.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>A <see cref="Task"/> that completes when the middleware has completed processing.</returns>
    public async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var localizationOptions = new RequestLocalizationOptions();
        var supportedCultures = await _localizationService.GetSupportedCulturesAsync();
        new LocalizationOptionsUpdater(localizationOptions, _cultureOptions.IgnoreSystemSettings)
            .SetDefaultCulture(await _localizationService.GetDefaultCultureAsync())
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);

        var requestCulture = localizationOptions.DefaultRequestCulture;

        IRequestCultureProvider winningProvider = null;

        if (localizationOptions.RequestCultureProviders != null)
        {
            foreach (var provider in localizationOptions.RequestCultureProviders)
            {
                var providerResultCulture = await provider.DetermineProviderCultureResult(context);
                if (providerResultCulture == null)
                {
                    continue;
                }

                var cultures = providerResultCulture.Cultures;
                var uiCultures = providerResultCulture.UICultures;

                CultureInfo cultureInfo = null;
                CultureInfo uiCultureInfo = null;
                if (localizationOptions.SupportedCultures != null)
                {
                    cultureInfo = GetCultureInfo(
                        cultures,
                        localizationOptions.SupportedCultures,
                        localizationOptions.FallBackToParentCultures);

                    if (cultureInfo == null)
                    {
                        _logger.UnsupportedCultures(provider.GetType().Name, cultures);
                    }
                }

                if (localizationOptions.SupportedUICultures != null)
                {
                    uiCultureInfo = GetCultureInfo(
                        uiCultures,
                        localizationOptions.SupportedUICultures,
                        localizationOptions.FallBackToParentUICultures);

                    if (uiCultureInfo == null)
                    {
                        _logger.UnsupportedUICultures(provider.GetType().Name, uiCultures);
                    }
                }

                if (cultureInfo == null && uiCultureInfo == null)
                {
                    continue;
                }

                cultureInfo ??= localizationOptions.DefaultRequestCulture.Culture;
                uiCultureInfo ??= localizationOptions.DefaultRequestCulture.UICulture;

                var result = new RequestCulture(cultureInfo, uiCultureInfo);
                requestCulture = result;
                winningProvider = provider;

                break;
            }
        }

        context.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(requestCulture, winningProvider));

        SetCurrentThreadCulture(requestCulture);

        if (localizationOptions.ApplyCurrentCultureToResponseHeaders)
        {
            var headers = context.Response.Headers;
            headers.ContentLanguage = requestCulture.UICulture.Name;
        }

        await _next(context);
    }

    private static void SetCurrentThreadCulture(RequestCulture requestCulture)
    {
        CultureInfo.CurrentCulture = requestCulture.Culture;
        CultureInfo.CurrentUICulture = requestCulture.UICulture;
    }

    private static CultureInfo GetCultureInfo(
        IList<StringSegment> cultureNames,
        IList<CultureInfo> supportedCultures,
        bool fallbackToParentCultures)
    {
        foreach (var cultureName in cultureNames)
        {
            // Allow empty string values as they map to InvariantCulture, whereas null culture values will throw in
            // the CultureInfo ctor
            if (cultureName != null)
            {
                var cultureInfo = GetCultureInfo(cultureName, supportedCultures, fallbackToParentCultures, currentDepth: 0);
                if (cultureInfo != null)
                {
                    return cultureInfo;
                }
            }
        }

        return null;
    }

    private static CultureInfo GetCultureInfo(
        StringSegment cultureName,
        IList<CultureInfo> supportedCultures,
        bool fallbackToParentCultures,
        int currentDepth)
    {
        // If the cultureName is an empty string there
        // is no chance we can resolve the culture info.
        if (cultureName.Equals(string.Empty))
        {
            return null;
        }

        var culture = GetCultureInfo(cultureName, supportedCultures);

        if (culture == null && fallbackToParentCultures && currentDepth < MaxCultureFallbackDepth)
        {
            try
            {
                culture = CultureInfo.GetCultureInfo(cultureName.ToString());

                culture = GetCultureInfo(culture.Parent.Name, supportedCultures, fallbackToParentCultures, currentDepth + 1);
            }
            catch (CultureNotFoundException)
            {
            }
        }

        return culture;
    }

    private static CultureInfo GetCultureInfo(StringSegment name, IList<CultureInfo> supportedCultures)
    {
        // Allow only known culture names as this API is called with input from users (HTTP requests) and
        // creating CultureInfo objects is expensive and we don't want it to throw either.
        if (name == null || supportedCultures == null)
        {
            return null;
        }

        var culture = supportedCultures.FirstOrDefault(
            supportedCulture => StringSegment.Equals(supportedCulture.Name, name, StringComparison.OrdinalIgnoreCase));

        if (culture == null)
        {
            return null;
        }

        return CultureInfo.ReadOnly(culture);
    }
}

internal static partial class RequestCultureProviderLoggerExtensions
{
    [LoggerMessage(1, LogLevel.Debug, "{requestCultureProvider} returned the following unsupported cultures '{cultures}'.", EventName = "UnsupportedCulture")]
    public static partial void UnsupportedCultures(this ILogger logger, string requestCultureProvider, IList<StringSegment> cultures);

    [LoggerMessage(2, LogLevel.Debug, "{requestCultureProvider} returned the following unsupported UI Cultures '{uiCultures}'.", EventName = "UnsupportedUICulture")]
    public static partial void UnsupportedUICultures(this ILogger logger, string requestCultureProvider, IList<StringSegment> uiCultures);
}
