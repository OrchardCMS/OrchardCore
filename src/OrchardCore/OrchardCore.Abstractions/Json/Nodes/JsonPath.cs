#nullable enable

namespace System.Text.Json.Nodes;

/// <summary>
/// A minimal in-house implementation of JSONPath querying for <see cref="JsonNode"/> objects.
/// Supports property access, array indexing, and recursive descent ($..property).
/// </summary>
internal static class JsonPathQuery
{
    /// <summary>
    /// Evaluates a JSONPath expression against the given root node and returns the first matching node.
    /// </summary>
    /// <param name="root">The root <see cref="JsonNode"/> to query.</param>
    /// <param name="path">The JSONPath expression.</param>
    /// <returns>The first matching <see cref="JsonNode"/>, or null if not found.</returns>
    public static JsonNode? Evaluate(JsonNode? root, string path)
    {
        if (root is null || string.IsNullOrEmpty(path))
        {
            return root;
        }

        // Handle recursive descent operator ($..property)
        if (path.StartsWith("$..", StringComparison.Ordinal))
        {
            var propertyName = path[3..];
            return FindRecursive(root, propertyName);
        }

        // Handle root-prefixed paths ($. or just $)
        if (path.StartsWith("$.", StringComparison.Ordinal))
        {
            path = path[2..];
        }
        else if (path == "$")
        {
            return root;
        }

        // Handle empty path after normalization
        if (string.IsNullOrEmpty(path))
        {
            return root;
        }

        // Parse and evaluate the path segments
        var segments = ParsePathSegments(path);
        return NavigatePath(root, segments);
    }

    /// <summary>
    /// Parses a path string into individual segments.
    /// Supports dot notation and bracket notation for array indices.
    /// </summary>
    private static List<PathSegment> ParsePathSegments(string path)
    {
        var segments = new List<PathSegment>();
        var i = 0;

        while (i < path.Length)
        {
            // Skip leading dots
            if (path[i] == '.')
            {
                i++;
                continue;
            }

            // Check for bracket notation for array index
            if (path[i] == '[')
            {
                var endBracket = path.IndexOf(']', i);
                if (endBracket == -1)
                {
                    break; // Malformed path
                }

                var indexStr = path[(i + 1)..endBracket];
                if (int.TryParse(indexStr, out var index))
                {
                    segments.Add(new PathSegment(index));
                }
                i = endBracket + 1;
                continue;
            }

            // Parse property name until next delimiter
            var start = i;
            while (i < path.Length && path[i] != '.' && path[i] != '[')
            {
                i++;
            }

            if (i > start)
            {
                var propertyName = path[start..i];
                segments.Add(new PathSegment(propertyName));
            }
        }

        return segments;
    }

    /// <summary>
    /// Navigates through the JSON structure following the path segments.
    /// </summary>
    private static JsonNode? NavigatePath(JsonNode? current, List<PathSegment> segments)
    {
        foreach (var segment in segments)
        {
            if (current is null)
            {
                return null;
            }

            if (segment.IsIndex)
            {
                if (current is JsonArray jsonArray && segment.Index >= 0 && segment.Index < jsonArray.Count)
                {
                    current = jsonArray[segment.Index];
                }
                else
                {
                    return null;
                }
            }
            else if (segment.PropertyName is not null)
            {
                if (current is JsonObject jsonObject)
                {
                    current = jsonObject.TryGetPropertyValue(segment.PropertyName, out var value) ? value : null;
                }
                else
                {
                    return null;
                }
            }
        }

        return current;
    }

    /// <summary>
    /// Recursively searches for a property with the specified name in the entire JSON tree.
    /// Returns the first match found (depth-first).
    /// </summary>
    private static JsonNode? FindRecursive(JsonNode node, string propertyName)
    {
        if (node is JsonObject jsonObject)
        {
            // Check if this object has the property
            if (jsonObject.TryGetPropertyValue(propertyName, out var value))
            {
                return value;
            }

            // Search in child nodes
            foreach (var property in jsonObject)
            {
                if (property.Value is not null)
                {
                    var result = FindRecursive(property.Value, propertyName);
                    if (result is not null)
                    {
                        return result;
                    }
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            // Search in array elements
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    var result = FindRecursive(item, propertyName);
                    if (result is not null)
                    {
                        return result;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Represents a segment in a JSONPath expression.
    /// </summary>
    private readonly struct PathSegment
    {
        public string? PropertyName { get; }
        public int Index { get; }
        public bool IsIndex { get; }

        public PathSegment(string propertyName)
        {
            PropertyName = propertyName;
            Index = -1;
            IsIndex = false;
        }

        public PathSegment(int index)
        {
            PropertyName = null;
            Index = index;
            IsIndex = true;
        }
    }
}
