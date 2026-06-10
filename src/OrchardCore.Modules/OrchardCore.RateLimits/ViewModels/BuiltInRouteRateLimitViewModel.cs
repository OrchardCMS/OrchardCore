namespace OrchardCore.RateLimits.ViewModels;

public class BuiltInRouteRateLimitViewModel
{
    public string RouteName { get; set; }

    public IList<string> Methods { get; set; } = [];
}
