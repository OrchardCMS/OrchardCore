using System.Collections.Generic;

namespace OrchardCore.DisplayManagement
{
    public interface INamedEnumerable<T> : IEnumerable<T>
    {
        IList<T> Positional { get; }
        IDictionary<string, T> Named { get; }
    }
}
