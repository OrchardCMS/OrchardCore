using System.Collections;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Environment.Shell.Configuration.Internal;

internal class UpdatableDataProvider : IConfigurationProvider, IConfigurationSource, IEnumerable<KeyValuePair<string, string>>
{
    public UpdatableDataProvider() => Data = new(StringComparer.OrdinalIgnoreCase);

    public UpdatableDataProvider(IEnumerable<KeyValuePair<string, string>> initialData)
    {
        initialData ??= [];
        Data = new(initialData, StringComparer.OrdinalIgnoreCase);
    }

    protected ConcurrentDictionary<string, string> Data { get; set; }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => Data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public virtual bool TryGet(string key, out string value) => Data.TryGetValue(key, out value);

    public virtual void Set(string key, string value) => Data[key] = value;

    public virtual void Load()
    {
    }

    public virtual IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
    {
        var prefix = parentPath == null ? string.Empty : parentPath + ConfigurationPath.KeyDelimiter;

        return Data
            .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(kv => Segment(kv.Key, prefix.Length))
            .Concat(earlierKeys)
            .OrderBy(k => k, ConfigurationKeyComparer.Instance);
    }

    private static string Segment(string key, int prefixLength)
    {
        var indexOf = key.IndexOf(ConfigurationPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);
        return indexOf < 0 ? key[prefixLength..] : key[prefixLength..indexOf];
    }

    public IChangeToken GetReloadToken() => NullChangeToken.Singleton;

    public override string ToString() => $"{GetType().Name}";

    public IConfigurationProvider Build(IConfigurationBuilder builder) => this;
}
