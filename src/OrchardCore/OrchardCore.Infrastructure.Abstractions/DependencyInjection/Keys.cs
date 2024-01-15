using System.Collections.Generic;

namespace OrchardCore.DependencyInjection;

public class Keys<T> : List<object>
{
    public Keys(IEnumerable<object> collection) : base(collection)
    {
    }
}
