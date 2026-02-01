namespace OrchardCore.Deployment.ViewModels;

public class ImportJsonViewModel
{
    public string Json { get; set; }

    /// <summary>
    /// Gets or sets the JSON Schema for recipe validation and editor intellisense.
    /// </summary>
    public string Schema { get; set; }
}
