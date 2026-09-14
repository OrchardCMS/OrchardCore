using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Describes the breadcrumb trails of the templates screens, for the templates of the site and for the templates of
/// the admin alike.
/// </summary>
public sealed class TemplatesBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_listRouteValues = new()
    {
        { "area", "OrchardCore.Templates" },
    };

    internal readonly IStringLocalizer S;

    public TemplatesBreadcrumbProvider(IStringLocalizer<TemplatesBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case TemplatesBreadcrumbs.List:
                AddList(builder);
                break;

            case TemplatesBreadcrumbs.Create:
                AddList(builder);
                builder.Add(S["Create Template"], item => item.Id("Template"));
                break;

            case TemplatesBreadcrumbs.Edit:
                AddList(builder);
                builder.Add(IsAdminTemplates(builder) ? S["Edit Admin Template"] : S["Edit Template"],
                    item => item.Id("Template"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
    {
        var adminTemplates = IsAdminTemplates(builder);

        builder.Add(adminTemplates ? S["Admin Templates"] : S["Templates"], item => item
            .Id("Templates")
            .Action(adminTemplates ? "Admin" : "Index", "Template", s_listRouteValues)
            .Permission(Permissions.ManageTemplates));
    }

    private static bool IsAdminTemplates(BreadcrumbBuilder builder)
        => builder.GetData<bool>(TemplatesBreadcrumbs.AdminTemplatesKey);
}
