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
    private readonly bool _fallBackToParentCulture;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance of <see cref="PortableObjectStringLocalizerFactory"/>.
    /// </summary>
    /// <param name="localizationManager"></param>
    /// <param name="requestLocalizationOptions"></param>
    /// <param name="logger"></param>
    public PortableObjectStringLocalizerFactory(
        ILocalizationManager localizationManager,
        IOptions<RequestLocalizationOptions> requestLocalizationOptions,
        ILogger<PortableObjectStringLocalizerFactory> logger)
    {
        _localizationManager = localizationManager;
        _fallBackToParentCulture = requestLocalizationOptions.Value.FallBackToParentUICultures;
        _logger = logger;
    }

    /// <inheritedoc />
    public IStringLocalizer Create(Type resourceSource)
    {
        ArgumentNullException.ThrowIfNull(resourceSource);

        return _localizerCache.GetOrAdd(new(resourceSource, null, null), static (key, args) =>
            new PortableObjectStringLocalizer(TryFixInnerClassPath(key.ResourceSource.FullName), args._localizationManager, args._fallBackToParentCulture, args._logger), (_localizationManager, _fallBackToParentCulture, _logger));
    }

    /// <inheritedoc />
    public IStringLocalizer Create(string baseName, string location)
    {
        ArgumentNullException.ThrowIfNull(baseName);
        ArgumentNullException.ThrowIfNull(location);

        return _localizerCache.GetOrAdd(new(null, baseName, location), static (key, args) =>
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

            return new PortableObjectStringLocalizer(relativeName, args._localizationManager, args._fallBackToParentCulture, args._logger);
        }, (_localizationManager, _fallBackToParentCulture, _logger));
    }

    // The context within inner class.
    private static string TryFixInnerClassPath(string context)
    {
        const char innerClassSeparator = '+';

        return context.Replace(innerClassSeparator, '.');
    }

    private readonly record struct LocalizerCacheKey(Type ResourceSource, string BaseName, string Location);
}
