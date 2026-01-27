namespace OrchardCore.Secrets.ViewModels;

public class SecretIndexViewModel
{
    public IList<SecretEntryViewModel> Secrets { get; set; } = [];
    public IList<SecretTypeViewModel> AvailableTypes { get; set; } = [];
}

public class SecretEntryViewModel
{
    public string Name { get; set; }
    public string Store { get; set; }
    public string Type { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
}
