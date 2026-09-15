using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.UrlRewriting.Services;

/// <summary>
/// Describes the breadcrumb trails of the URL rewriting screens.
/// </summary>
public sealed class UrlRewritingBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.UrlRewriting" },
    };

    internal readonly IStringLocalizer S;

    public UrlRewritingBreadcrumbProvider(IStringLocalizer<UrlRewritingBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case UrlRewritingConstants.List:
                AddList(builder);
                break;

            case UrlRewritingConstants.Create:
                AddList(builder);
                builder.Add(S["New '{0}' rule", builder.GetData<string>(UrlRewritingConstants.DisplayNameKey)],
                    item => item.Id("Rule"));
                break;

            case UrlRewritingConstants.Edit:
                AddList(builder);
                builder.Add(S["Edit '{0}' rule", builder.GetData<string>(UrlRewritingConstants.DisplayNameKey)],
                    item => item.Id("Rule"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["URL Rewriting Rules"], item => item
            .Id("UrlRewriting")
            .Action("Index", "Admin", s_routeValues)
            .Permission(UrlRewritingPermissions.ManageUrlRewritingRules));
}
