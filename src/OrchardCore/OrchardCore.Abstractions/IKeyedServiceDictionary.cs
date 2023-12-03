using System.Collections.Generic;

namespace OrchardCore;

public interface IKeyedServiceDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
{
}
