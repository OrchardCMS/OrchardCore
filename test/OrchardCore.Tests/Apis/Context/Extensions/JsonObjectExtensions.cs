namespace OrchardCore.Tests.Apis.Context;

public static class JsonObjectExtensions
{
    public static JsonNode GetNode(this JsonNode node, params object[] pathSegments)
    {
        ArgumentNullException.ThrowIfNull(node);

        for (var i = 0; i < pathSegments.Length; i++)
        {
            var segment = pathSegments[i] ?? throw new InvalidOperationException($"Path segment #{i} is null.");
            var child = segment is int index ? node[index] : node[segment.ToString()!];

            node = child ?? throw new InvalidOperationException($"Couldn't find \"{segment}\" at {node.GetPath()}.");
        }

        return node;
    }

    public static string GetContentItemId(this JsonNode node) =>
        node["contentItemId"]?.GetValue<string>();
}
