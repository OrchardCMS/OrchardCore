using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Deployment.ViewModels;

public class ImportJsonViewModel
{
    public string Json { get; set; }

    /// <summary>
    /// Gets or sets the editor of the recipe to import, built by the display drivers of <see cref="ImportJsonViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
