using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Secrets.ViewModels;

public class SecretEditViewModel
{
    public bool IsNew { get; set; }

    [Required]
    public string Name { get; set; }

    public string Store { get; set; }

    public string Description { get; set; }

    [Required]
    public string SecretType { get; set; }

    /// <summary>
    /// For TextSecret: the secret value.
    /// </summary>
    public string SecretValue { get; set; }

    public IList<string> AvailableStores { get; set; } = [];

    public IList<string> AvailableTypes { get; set; } = [];
}
