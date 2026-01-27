using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

    public string SecretTypeDisplayName { get; set; }

    public IList<string> AvailableStores { get; set; } = [];

    /// <summary>
    /// The editor shape built by the display driver.
    /// </summary>
    [BindNever]
    public dynamic Editor { get; set; }
}
