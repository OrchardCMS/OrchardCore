namespace OrchardCore.Secrets.ViewModels;

public class SecretTypeSelectionViewModel
{
    public IList<SecretTypeViewModel> AvailableTypes { get; set; } = [];
}

public class SecretTypeViewModel
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
}
