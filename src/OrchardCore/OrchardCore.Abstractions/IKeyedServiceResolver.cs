using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore;

public interface IKeyedServiceResolver
{
    IEnumerable<KeyValuePair<TKey, TService>> GetServices<TKey, TService>();
}

public static class KeyedServiceResolverExtensions
{
    public static IReadOnlyDictionary<TKey, TService> GetServicesAsDictionary<TKey, TService>(this IKeyedServiceResolver resolver)
    {
        var services = resolver.GetServices<TKey, TService>();

        return services.GroupBy(s => s.Key)
            .ToFrozenDictionary(s => s.Key, s => s.Select(s => s.Value).Last());
    }
}
