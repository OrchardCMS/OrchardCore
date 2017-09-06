using System.Collections.Generic;

namespace OrchardCore.DisplayManagement
{
    public interface INamedEnumerable<T> : IEnumerable<T>
    {
        IEnumerable<T> Positional { get; }
        IDictionary<string, T> Named { get; }
    }
}