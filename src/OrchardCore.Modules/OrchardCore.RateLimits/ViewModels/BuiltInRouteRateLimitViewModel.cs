namespace OrchardCore.RateLimits.ViewModels;

public class BuiltInRouteRateLimitViewModel
{
    public string Path { get; set; }

    public IList<string> Methods { get; set; } = [];
}
