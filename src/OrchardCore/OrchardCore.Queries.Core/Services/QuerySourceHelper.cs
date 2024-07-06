using System.Text.Json.Nodes;

namespace OrchardCore.Queries.Core.Services;

public static class QuerySourceHelper
{
    public static Query CreateQuery(string source, bool canReturnContentItems = true, JsonNode data = null)
    {
        var query = new Query()
        {
            Source = source,
            CanReturnContentItems = canReturnContentItems,
        };

        if (data != null)
        {
            var name = data[nameof(Query.Name)];

            if (name != null)
            {
                query.Name = name.GetValue<string>();
            }
            var schema = data[nameof(Query.Schema)];

            if (schema != null)
            {
                query.Schema = schema.GetValue<string>();
            }

            // For backward compatibility, we use the key 'ReturnDocuments'.
            var returnDocuments = data["ReturnDocuments"];

            if (returnDocuments != null)
            {
                query.ReturnContentItems = returnDocuments.GetValue<bool>();
            }

            var returnContentItems = data[nameof(Query.ReturnContentItems)];

            if (returnContentItems != null)
            {
                query.ReturnContentItems = returnContentItems.GetValue<bool>();
            }
        }

        return query;
    }
}
