using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.AuditTrail.Navigation;

/// <summary>
/// Describes the breadcrumb trails of the audit trail screens.
/// </summary>
public sealed class AuditTrailBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.AuditTrail" },
    };

    internal readonly IStringLocalizer S;

    public AuditTrailBreadcrumbProvider(IStringLocalizer<AuditTrailBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case AuditTrailBreadcrumbs.List:
                AddList(builder);
                break;

            case AuditTrailBreadcrumbs.Display:
                AddList(builder);
                builder.Add(S["Audit Trail Event"], item => item.Id("Event"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Audit Trail"], item => item
            .Id("AuditTrail")
            .Action("Index", "Admin", s_routeValues)
            .Permission(AuditTrailPermissions.ViewAuditTrail));
}
