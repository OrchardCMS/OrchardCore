using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Roles.Controllers;

namespace OrchardCore.Roles.Services;

/// <summary>
/// Describes the breadcrumb trails of the roles screens.
/// </summary>
public sealed class RolesBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_listRouteValues = new()
    {
        { "area", "OrchardCore.Roles" },
    };

    internal readonly IStringLocalizer S;

    public RolesBreadcrumbProvider(IStringLocalizer<RolesBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case RolesConstants.List:
                AddList(builder);
                break;

            case RolesConstants.Create:
                AddList(builder);
                builder.Add(S["Create Role"], item => item.Id("Role"));
                break;

            case RolesConstants.Edit:
                AddList(builder);
                builder.Add(S["Edit '{0}' Role", GetRoleName(builder)], item => item.Id("Role"));
                break;

            case RolesConstants.Clone:
                AddList(builder);
                builder.Add(S["Clone '{0}' Role", GetRoleName(builder)], item => item.Id("Role"));
                break;

            case RolesConstants.Display:
                AddList(builder);
                builder.Add(GetRoleName(builder), item => item.Id("Role"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Roles"], item => item
            .Id("Roles")
            .Action(nameof(AdminController.Index), typeof(AdminController).ControllerName(), s_listRouteValues)
            .Permission(RolesPermissions.ViewRoles));

    private static string GetRoleName(BreadcrumbBuilder builder)
        => builder.GetData<string>(RolesConstants.RoleNameKey);
}
