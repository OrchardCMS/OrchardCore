using System.Collections.Generic;

namespace OrchardCore;

public interface IKeyedServiceResolver
{
    IEnumerable<KeyValuePair<TKey, TService>> GetServices<TKey, TService>();
}
