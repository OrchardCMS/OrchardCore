using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

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

    /// <summary>
    /// Gets or sets the editor of the scope, built by the display drivers of <see cref="EditOpenIdScopeViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
