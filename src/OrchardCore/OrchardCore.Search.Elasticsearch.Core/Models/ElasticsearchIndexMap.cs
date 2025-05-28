using Elastic.Clients.Elasticsearch.Mapping;

namespace OrchardCore.Search.Elasticsearch.Models;

public sealed class ElasticsearchIndexMap
{
    public string KeyFieldName { get; set; }

    public TypeMapping Mapping { get; set; }

    public List<string> GetFieldPaths()
    {
        var fieldPaths = new List<string>();

        Traverse(Mapping.Properties, string.Empty, fieldPaths);

        return fieldPaths;
    }

    private static void Traverse(Properties props, string parentPath, List<string> result)
    {
        foreach (var kvp in props)
        {
            var fieldName = kvp.Key.Name;
            var property = kvp.Value;

            var fullPath = string.IsNullOrEmpty(parentPath) ? fieldName : $"{parentPath}.{fieldName}";

            if (property is ObjectProperty objectProp && objectProp.Properties != null)
            {
                Traverse(objectProp.Properties, fullPath, result);
            }
            else if (property is NestedProperty nestedProp && nestedProp.Properties != null)
            {
                Traverse(nestedProp.Properties, fullPath, result);
            }
            else
            {
                result.Add(fullPath);
            }
        }
    }
}
