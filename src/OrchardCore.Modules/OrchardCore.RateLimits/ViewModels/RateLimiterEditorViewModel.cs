namespace OrchardCore.RateLimits.ViewModels;

public class RateLimiterEditorViewModel
{
    public string DisplayName { get; set; }

    public string Description { get; set; }

    public string DocumentationUrl { get; set; }

    public dynamic Editor { get; set; }
}
