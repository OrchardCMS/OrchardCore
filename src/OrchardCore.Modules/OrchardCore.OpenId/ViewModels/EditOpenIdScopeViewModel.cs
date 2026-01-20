using System.ComponentModel.DataAnnotations;

namespace OrchardCore.OpenId.ViewModels;

public class EditOpenIdScopeViewModel
{
    public string Description { get; set; }

    [Required]
    public string DisplayName { get; set; }

    [Required]
    public string Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string Resources { get; set; }
}
