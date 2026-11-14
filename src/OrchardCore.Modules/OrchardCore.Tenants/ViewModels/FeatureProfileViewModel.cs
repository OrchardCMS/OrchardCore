using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Tenants.ViewModels;

public class FeatureProfileViewModel
{
    [Required]
    public string Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string FeatureRules { get; set; }

    /// <summary>
    /// Gets or sets the editor of the feature profile, built by the display drivers of <see cref="FeatureProfileViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
