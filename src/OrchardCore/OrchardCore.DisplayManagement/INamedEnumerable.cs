namespace OrchardCore.DisplayManagement;

#pragma warning disable CA1710 // Identifiers should have correct suffix
public interface INamedEnumerable<T> : IEnumerable<T>, IReadOnlyCollection<T>
#pragma warning restore CA1710 // Identifiers should have correct suffix
{
    IList<T> Positional { get; }
    IDictionary<string, T> Named { get; }
}
