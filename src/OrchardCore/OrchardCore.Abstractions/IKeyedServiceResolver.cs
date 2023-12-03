using System.Collections.Generic;

namespace OrchardCore;

public interface IKeyedServiceResolver
{
    IReadOnlyDictionary<TKey, TService> GetServices<TKey, TService>();
}
