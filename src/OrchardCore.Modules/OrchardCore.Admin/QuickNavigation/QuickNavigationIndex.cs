using System.Collections.Concurrent;

namespace OrchardCore.Admin.QuickNavigation;

public sealed class QuickNavigationIndex : IQuickNavigationIndex
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<string>> _entries = new(StringComparer.Ordinal);

    public IReadOnlyList<string> GetEntryIds(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        return _entries.TryGetValue(source, out var entries) ? entries : [];
    }

    public void Replace(string source, IEnumerable<string> entryIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(entryIds);

        var entries = entryIds.Distinct(StringComparer.Ordinal).ToArray();
        foreach (var entry in entries)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entry);
        }

        if (entries.Length == 0)
        {
            _entries.TryRemove(source, out _);
        }
        else
        {
            _entries[source] = Array.AsReadOnly(entries);
        }
    }
}
