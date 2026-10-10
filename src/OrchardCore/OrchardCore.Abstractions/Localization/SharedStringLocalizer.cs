using System.Collections.Concurrent;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization;

public static class SharedStringLocalizer
{
    private static IStringLocalizerFactory _stringLocalizerFactory;
    private static readonly ConcurrentDictionary<Type, Lazy<IStringLocalizer>> _stringLocalizersCache = new();

    public static void Initialize(IStringLocalizerFactory stringLocalizerFactory)
        => _stringLocalizerFactory = stringLocalizerFactory ?? throw new ArgumentNullException(nameof(stringLocalizerFactory));

    public static DeferredLocalizedString Get(Type resourceType, string key, params object[] args)
        => new DeferredLocalizedString(resourceType, key, args);

    public static DeferredLocalizedString Get(string key, params object[] args) => Get<SharedResources>(key, args);

    public static DeferredLocalizedString Get<TType>(string key, params object[] args)
        => new DeferredLocalizedString(typeof(TType), key, args);

    internal static IStringLocalizer GetLocalizer(Type resourceType)
    {
        if (_stringLocalizerFactory == null)
        {
            throw new InvalidOperationException("SharedStringLocalizer is not initialized. Call Initialize() first.");
        }

        return _stringLocalizersCache.GetOrAdd(resourceType, t =>
            new Lazy<IStringLocalizer>(() => _stringLocalizerFactory.Create(t), isThreadSafe: true)).Value;
    }
}
