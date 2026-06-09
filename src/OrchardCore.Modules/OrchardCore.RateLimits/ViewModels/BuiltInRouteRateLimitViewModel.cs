namespace OrchardCore.RateLimits.ViewModels;

public class BuiltInRouteRateLimitViewModel
{
    public string Path { get; set; }

    public string RouteName { get; set; }

    public IList<string> Methods { get; set; } = [];
}
