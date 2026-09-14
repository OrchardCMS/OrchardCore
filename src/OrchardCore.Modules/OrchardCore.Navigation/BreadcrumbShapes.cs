using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Navigation;

public sealed class BreadcrumbShapes : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Breadcrumb")
            .OnDisplaying(displaying =>
            {
                var breadcrumb = displaying.Shape;
                var name = breadcrumb.GetProperty<string>("Name");

                breadcrumb.Classes.Add("oc-breadcrumb");
                breadcrumb.Classes.Add("breadcrumb-" + name.HtmlClassify());

                // Breadcrumb__[Name] e.g. Breadcrumb-Contents_Edit.cshtml
                breadcrumb.Metadata.Alternates.AddRange(BreadcrumbAlternatesFactory.GetBreadcrumbAlternates(name));
            });

        builder.Describe("BreadcrumbItem")
            .OnDisplaying(displaying =>
            {
                var breadcrumbItem = displaying.Shape;

                var name = breadcrumbItem is BreadcrumbItemViewModel viewModel
                    ? viewModel.Name
                    : breadcrumbItem.GetProperty<string>(nameof(BreadcrumbItemViewModel.Name));

                var id = breadcrumbItem is BreadcrumbItemViewModel typedViewModel
                    ? typedViewModel.Item?.Id
                    : breadcrumbItem.GetProperty<BreadcrumbItem>(nameof(BreadcrumbItemViewModel.Item))?.Id;

                // BreadcrumbItem__[Name] e.g. BreadcrumbItem-Contents_Edit.cshtml
                // BreadcrumbItem__[Id] e.g. BreadcrumbItem-ContentTypes.cshtml
                // BreadcrumbItem__[Name]__[Id] e.g. BreadcrumbItem-Contents_Edit-ContentTypes.cshtml
                breadcrumbItem.Metadata.Alternates.AddRange(BreadcrumbAlternatesFactory.GetBreadcrumbItemAlternates(name, id));
            });

        return ValueTask.CompletedTask;
    }
}
