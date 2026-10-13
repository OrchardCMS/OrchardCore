using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization.PortableObject;

/// <summary>
/// Represents a <see cref="IStringLocalizerFactory"/> for portable objects.
/// </summary>
public class PortableObjectStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly ILocalizationManager _localizationManager;
    private readonly ConcurrentDictionary<LocalizerCacheKey, PortableObjectStringLocalizer> _localizerCache = new();
    private readonly IOptionsMonitor<RequestLocalizationOptions> _requestLocalizationOptions;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance of <see cref="PortableObjectStringLocalizerFactory"/>.
    /// </summary>
    /// <param name="localizationManager">The <see cref="ILocalizationManager"/>.</param>
    /// <param name="requestLocalizationOptions">
    /// The <see cref="IOptionsMonitor{TOptions}"/> of the <see cref="RequestLocalizationOptions"/>, whose current
    /// <see cref="RequestLocalizationOptions.FallBackToParentUICultures"/> value is used by the created localizers.
    /// </param>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
    public PortableObjectStringLocalizerFactory(
        ILocalizationManager localizationManager,
        IOptionsMonitor<RequestLocalizationOptions> requestLocalizationOptions,
        ILogger<PortableObjectStringLocalizerFactory> logger)
    {
        _localizationManager = localizationManager;
        _requestLocalizationOptions = requestLocalizationOptions;
        _logger = logger;
    }

    /// <inheritedoc />
    public IStringLocalizer Create(Type resourceSource)
    {
        ArgumentNullException.ThrowIfNull(resourceSource);

        var fallBackToParentCulture = _requestLocalizationOptions.CurrentValue.FallBackToParentUICultures;

        return _localizerCache.GetOrAdd(new(resourceSource, null, null, fallBackToParentCulture), static (key, args) =>
            new PortableObjectStringLocalizer(TryFixInnerClassPath(key.ResourceSource.FullName), args._localizationManager, key.FallBackToParentCulture, args._logger), (_localizationManager, _logger));
    }

    /// <inheritedoc />
    public IStringLocalizer Create(string baseName, string location)
    {
        ArgumentNullException.ThrowIfNull(baseName);
        ArgumentNullException.ThrowIfNull(location);

        var fallBackToParentCulture = _requestLocalizationOptions.CurrentValue.FallBackToParentUICultures;

        return _localizerCache.GetOrAdd(new(null, baseName, location, fallBackToParentCulture), static (key, args) =>
        {
            var normalizedBaseName = TryFixInnerClassPath(key.BaseName);
            var index = 0;
            if (normalizedBaseName.StartsWith(key.Location, StringComparison.OrdinalIgnoreCase))
            {
                index = key.Location.Length;
            }

            if (normalizedBaseName.Length > index && normalizedBaseName[index] == '.')
            {
                index += 1;
            }

            if (normalizedBaseName.Length > index && normalizedBaseName.IndexOf("Areas.", index, StringComparison.Ordinal) == index)
            {
                index += "Areas.".Length;
            }

            var relativeName = normalizedBaseName[index..];

            return new PortableObjectStringLocalizer(relativeName, args._localizationManager, key.FallBackToParentCulture, args._logger);
        }, (_localizationManager, _logger));
    }

    // The context within inner class.
    private static string TryFixInnerClassPath(string context)
    {
        const char innerClassSeparator = '+';

        return context.Replace(innerClassSeparator, '.');
    }

    // The fallback flag is part of the key so that the localizers follow the changes of the options.
    private readonly record struct LocalizerCacheKey(Type ResourceSource, string BaseName, string Location, bool FallBackToParentCulture);
}
