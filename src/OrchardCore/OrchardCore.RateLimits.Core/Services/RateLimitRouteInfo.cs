namespace OrchardCore.RateLimits.Services;

public class RateLimitRouteInfo
{
    public string Name { get; set; }

    public string Path { get; set; }

    public IReadOnlyList<string> Methods { get; set; } = [];
}
