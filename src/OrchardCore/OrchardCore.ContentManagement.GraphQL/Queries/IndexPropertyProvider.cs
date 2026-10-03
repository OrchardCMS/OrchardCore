using System.Reflection;
using YesSql.Indexes;

namespace OrchardCore.ContentManagement.GraphQL.Queries;

public class IndexPropertyProvider<T> : IIndexPropertyProvider where T : MapIndex
{
    private static readonly Dictionary<string, string> s_indexProperties = new(StringComparer.OrdinalIgnoreCase);
    private static readonly string s_indexName;

    static IndexPropertyProvider()
    {
        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase))
        {
            s_indexProperties[property.Name] = property.Name;
        }

        s_indexName = typeof(T).Name;
    }

    public string IndexName => s_indexName;

    public bool TryGetValue(string propertyName, out string indexPropertyName)
    {
        return s_indexProperties.TryGetValue(propertyName, out indexPropertyName);
    }
}
