namespace OrchardCore.DataLocalization.ViewModels;

public class TranslationsDeploymentStepViewModel
{
    public bool IncludeAll { get; set; } = true;

    public string[] Cultures { get; set; } = [];

    public string[] Categories { get; set; } = [];

    // Available options for selection.
    public string[] AllCultures { get; set; } = [];

    public string[] AllCategories { get; set; } = [];
}
