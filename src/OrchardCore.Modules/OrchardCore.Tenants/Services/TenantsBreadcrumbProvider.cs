using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Tenants.Services;

/// <summary>
/// Describes the breadcrumb trails of the tenants and feature profiles screens.
/// </summary>
public sealed class TenantsBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Tenants" },
    };

    internal readonly IStringLocalizer S;

    public TenantsBreadcrumbProvider(IStringLocalizer<TenantsBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case TenantsConstants.List:
                AddTenants(builder);
                break;

            case TenantsConstants.Create:
                AddTenants(builder);
                builder.Add(S["Create Tenant"], item => item.Id("Tenant"));
                break;

            case TenantsConstants.Edit:
                AddTenants(builder);
                builder.Add(S["Edit Tenant"], item => item.Id("Tenant"));
                break;

            case TenantsConstants.FeatureProfiles:
                AddFeatureProfiles(builder);
                break;

            case TenantsConstants.FeatureProfilesCreate:
                AddFeatureProfiles(builder);
                builder.Add(S["Create Feature Profile"], item => item.Id("FeatureProfile"));
                break;

            case TenantsConstants.FeatureProfilesEdit:
                AddFeatureProfiles(builder);
                builder.Add(S["Edit Feature Profile"], item => item.Id("FeatureProfile"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddTenants(BreadcrumbBuilder builder)
        => builder.Add(S["Tenants"], item => item
            .Id("Tenants")
            .Action("Index", "Admin", s_routeValues)
            .Permission(Permissions.ManageTenants));

    private void AddFeatureProfiles(BreadcrumbBuilder builder)
        => builder.Add(S["Feature Profiles"], item => item
            .Id("FeatureProfiles")
            .Action("Index", "FeatureProfiles", s_routeValues)
            .Permission(Permissions.ManageTenantFeatureProfiles));
}
