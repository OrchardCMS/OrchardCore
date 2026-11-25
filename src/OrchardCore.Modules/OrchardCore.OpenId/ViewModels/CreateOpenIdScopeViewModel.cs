using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.OpenId.ViewModels;

public class CreateOpenIdScopeViewModel
{
    public string Description { get; set; }

    [Required]
    public string DisplayName { get; set; }

    [Required]
    public string Name { get; set; }

    public string Resources { get; set; }

    /// <summary>
    /// Gets or sets the editor of a new scope, built by the display drivers of <see cref="CreateOpenIdScopeViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
