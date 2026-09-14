using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Users.Services;

/// <summary>
/// Describes the breadcrumb trails of the users screens.
/// </summary>
public sealed class UsersBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_listRouteValues = new()
    {
        { "area", UserConstants.Features.Users },
    };

    internal readonly IStringLocalizer S;

    public UsersBreadcrumbProvider(IStringLocalizer<UsersBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case UsersBreadcrumbs.List:
                AddList(builder);
                break;

            case UsersBreadcrumbs.Create:
                AddList(builder);
                builder.Add(S["Create User"], item => item.Id("User"));
                break;

            case UsersBreadcrumbs.Edit:
                AddList(builder);
                builder.Add(S["Edit User"], item => item.Id("User"));
                break;

            case UsersBreadcrumbs.EditPassword:
                AddList(builder);
                builder.Add(S["Change Password"], item => item.Id("User"));
                break;

            case UsersBreadcrumbs.Display:
                AddList(builder);
                builder.Add(S["View User"], item => item.Id("User"));
                break;

            case UsersBreadcrumbs.AuditTrail:
                builder.Add(S["Audit Trail User Event Settings"], item => item.Id("AuditTrail"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Users"], item => item
            .Id("Users")
            .Action("Index", "Admin", s_listRouteValues)
            .Permission(UsersPermissions.ListUsers));
}
