using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Deployment.ViewModels;

public class CreateDeploymentPlanViewModel
{
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the editor of the deployment plan, built by the display drivers of <see cref="CreateDeploymentPlanViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
