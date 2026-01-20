namespace OrchardCore.Features.ViewModels;

public class BulkActionViewModel
{
    public FeaturesBulkAction BulkAction { get; set; }
    public string[] FeatureIds { get; set; }
}

public enum FeaturesBulkAction
{
    None,
    Enable,
    Disable,
    Toggle
}
