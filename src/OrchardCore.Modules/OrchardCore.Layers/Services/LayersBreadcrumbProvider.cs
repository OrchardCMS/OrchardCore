using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Layers.Services;

/// <summary>
/// Describes the breadcrumb trails of the layers screens.
/// </summary>
public sealed class LayersBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Layers" },
    };

    internal readonly IStringLocalizer S;

    public LayersBreadcrumbProvider(IStringLocalizer<LayersBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case LayersConstants.List:
                AddList(builder);
                break;

            case LayersConstants.Create:
                AddList(builder);
                builder.Add(S["Create Layer"], item => item.Id("Layer"));
                break;

            case LayersConstants.Edit:
                AddList(builder);
                builder.Add(S["Edit Layer - {0}", builder.GetData<string>(LayersConstants.LayerNameKey)],
                    item => item.Id("Layer"));
                break;

            case LayersConstants.RuleCreate:
                AddList(builder);
                AddEditLayer(builder);
                builder.Add(S["Create Rule"], item => item.Id("Rule"));
                break;

            case LayersConstants.RuleEdit:
                AddList(builder);
                AddEditLayer(builder);
                builder.Add(S["Edit Rule"], item => item.Id("Rule"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Widgets and Layers"], item => item
            .Id("Layers")
            .Action("Index", "Admin", s_routeValues)
            .Permission(Permissions.ManageLayers));

    // A layer rule is only reached from its layer, so its trail leads back through the layer.
    private void AddEditLayer(BreadcrumbBuilder builder)
    {
        var layerName = builder.GetData<string>(LayersConstants.LayerNameKey);

        builder.Add(S["Edit Layer - {0}", layerName], item => item
            .Id("Layer")
            .Action("Edit", "Admin", new RouteValueDictionary(s_routeValues)
            {
                { "name", layerName },
            })
            .Permission(Permissions.ManageLayers));
    }
}
