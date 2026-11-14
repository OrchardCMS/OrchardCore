using Microsoft.Extensions.Localization;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Drivers;

/// <summary>
/// Builds the editor of the page importing a deployment package, whose fields are grouped in cards by their placement.
/// The package is posted as the <c>importedPackage</c> file, which the controller reads.
/// </summary>
internal sealed class ImportPackageDisplayDriver : DisplayDriver<ImportPackageViewModel>
{
    internal readonly IStringLocalizer S;

    public ImportPackageDisplayDriver(IStringLocalizer<ImportPackageDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ImportPackageViewModel model, BuildEditorContext context)
    {
        return Initialize<ImportPackageViewModel>("ImportPackageFile_Edit", m => { })
            .Location(l => l.Zone("Content", "1").Card(S["Package"], "10"));
    }
}
