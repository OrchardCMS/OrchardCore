using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Deployment.ViewModels;

/// <summary>
/// The page importing a deployment package.
/// </summary>
public class ImportPackageViewModel
{
    /// <summary>
    /// Gets or sets the editor of the package to import, built by the display drivers of <see cref="ImportPackageViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
