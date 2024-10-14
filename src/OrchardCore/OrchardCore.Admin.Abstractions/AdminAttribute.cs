using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.Admin;

/// <summary>
/// When applied to an action or a controller or a page model, intercepts any request to check whether it applies to the admin site.
/// If so it marks the request as such and ensures the user has the right to access it.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AdminAttribute : Attribute, IAsyncResourceFilter
{
    /// <summary>
    /// This may be used if the route name should be just the controller and action names stuck together. For
    /// example <c>~/Admin/AdminMenu/List</c> gets the route name <c>AdminMenuList</c>.
    /// </summary>
    public const string NameFromControllerAndAction = "{controller}{action}";

    /// <summary>
    /// Gets or sets the pattern which should be used as the path after the admin suffix. This is similar to the
    /// <see cref="RouteAttribute.Template"/>. When applying to a controller with multiple actions, the template
    /// should include the <c>{action}</c> expression. When applied to both the action and the controller, the
    /// action's template takes precedence. If it's <see langword="null"/> or empty for both, the fallback value
    /// is <c>{area}/{controller}/{action}/{id?}</c>.
    /// </summary>
    public string Template { get; set; }

    /// <summary>
    /// Gets or sets the name used when mapping the admin route. You can use <c>{area}</c>, <c>{controller}</c>,
    /// <c>{action}</c> in the string, which will be substituted. For performance reasons, these values are exact
    /// and shouldn't contain spaces. If empty when <see cref="Template"/> is used, the fallback is <see
    /// cref="ControllerActionDescriptor.DisplayName"/>.
    /// </summary>
    public string RouteName { get; set; }

    public AdminAttribute(string template = null, string routeName = null)
    {
        Template = template;
        RouteName = routeName;
    }

    public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        Apply(context.HttpContext);

        return next();
    }

    public static void Apply(HttpContext context)
    {
        // The value isn't important, it's just a marker object
        context.Items[typeof(AdminAttribute)] = null;
    }

    public static bool IsApplied(HttpContext context)
        => context.Items.ContainsKey(typeof(AdminAttribute));
}
