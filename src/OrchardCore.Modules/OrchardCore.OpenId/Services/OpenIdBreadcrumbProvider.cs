using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.OpenId.Services;

/// <summary>
/// Describes the breadcrumb trails of the OpenID screens.
/// </summary>
public sealed class OpenIdBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.OpenId" },
    };

    internal readonly IStringLocalizer S;

    public OpenIdBreadcrumbProvider(IStringLocalizer<OpenIdBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case OpenIdConstants.ApplicationsList:
                AddApplications(builder);
                break;

            case OpenIdConstants.ApplicationsCreate:
                AddApplications(builder);
                builder.Add(S["Create a new application"], item => item.Id("Application"));
                break;

            case OpenIdConstants.ApplicationsEdit:
                AddApplications(builder);
                builder.Add(S["Edit an application"], item => item.Id("Application"));
                break;

            case OpenIdConstants.ScopesList:
                AddScopes(builder);
                break;

            case OpenIdConstants.ScopesCreate:
                AddScopes(builder);
                builder.Add(S["Create a new scope"], item => item.Id("Scope"));
                break;

            case OpenIdConstants.ScopesEdit:
                AddScopes(builder);
                builder.Add(S["Edit a scope"], item => item.Id("Scope"));
                break;

            case OpenIdConstants.ServerConfiguration:
                builder.Add(S["Configure OpenID Connect server settings"], item => item.Id("ServerConfiguration"));
                break;

            case OpenIdConstants.ValidationConfiguration:
                builder.Add(S["Configure OpenID Connect validation settings"], item => item.Id("ValidationConfiguration"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddApplications(BreadcrumbBuilder builder)
        => builder.Add(S["Applications"], item => item
            .Id("OpenIdApplications")
            .Action("Index", "Application", s_routeValues)
            .Permission(OpenIdPermissions.ManageApplications));

    private void AddScopes(BreadcrumbBuilder builder)
        => builder.Add(S["Scopes"], item => item
            .Id("OpenIdScopes")
            .Action("Index", "Scope", s_routeValues)
            .Permission(OpenIdPermissions.ManageScopes));
}
