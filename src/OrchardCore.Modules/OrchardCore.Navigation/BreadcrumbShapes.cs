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

                // Breadcrumb__[Name] e.g. Breadcrumb-ContentsEdit.cshtml
                // Breadcrumb_[DisplayType] e.g. Breadcrumb.DetailAdmin.cshtml
                // Breadcrumb_[DisplayType]__[Name] e.g. Breadcrumb-ContentsEdit.DetailAdmin.cshtml
                breadcrumb.Metadata.Alternates.AddRange(
                    BreadcrumbAlternatesFactory.GetBreadcrumbAlternates(name, breadcrumb.Metadata.DisplayType));
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

                // BreadcrumbItem__[Name] e.g. BreadcrumbItem-ContentsEdit.cshtml
                // BreadcrumbItem__[Id] e.g. BreadcrumbItem-Dashboard.cshtml
                // BreadcrumbItem__[Name]__[Id] e.g. BreadcrumbItem-ContentsEdit-ContentItem.cshtml
                // BreadcrumbItem_[DisplayType]__[Name]__[Id] e.g. BreadcrumbItem-ContentsEdit-ContentItem.DetailAdmin.cshtml
                breadcrumbItem.Metadata.Alternates.AddRange(
                    BreadcrumbAlternatesFactory.GetBreadcrumbItemAlternates(name, id, breadcrumbItem.Metadata.DisplayType));
            });

        return ValueTask.CompletedTask;
    }
}
