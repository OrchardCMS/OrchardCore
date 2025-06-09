using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Routing;

public readonly struct RouteEndpointKey : IEquatable<RouteEndpointKey>
{
    public string Path { get; }
    public RouteValueDictionary RouteValues { get; }

    public RouteEndpointKey(string path, RouteValueDictionary routeValues)
    {
        Path = path ?? string.Empty;
        // Defensive copy to ensure immutability for dictionary key usage
        RouteValues = new RouteValueDictionary(routeValues);
    }

    public override bool Equals(object obj)
    {
        return obj is RouteEndpointKey && Equals((RouteEndpointKey)obj);
    }

    public bool Equals(RouteEndpointKey other)
    {
        if (!string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (RouteValues.Count != other.RouteValues.Count)
        {
            return false;
        }

        foreach (var kvp in RouteValues)
        {
            if (!other.RouteValues.TryGetValue(kvp.Key, out var otherValue))
            {
                return false;
            }

            if (!Equals(kvp.Value, otherValue))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Path);
        foreach (var kvp in RouteValues.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
        {
            hash = HashCode.Combine(hash,
                StringComparer.OrdinalIgnoreCase.GetHashCode(kvp.Key),
                kvp.Value?.GetHashCode() ?? 0);
        }

        return hash;
    }
    public static bool operator ==(RouteEndpointKey left, RouteEndpointKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RouteEndpointKey left, RouteEndpointKey right)
    {
        return !(left == right);
    }
}
