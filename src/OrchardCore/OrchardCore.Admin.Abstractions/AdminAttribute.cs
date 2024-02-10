using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Admin
{
    /// <summary>
    /// When applied to an action or a controller or a page model, intercepts any request to check whether it applies to the admin site.
    /// If so it marks the request as such and ensures the user has the right to access it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AdminAttribute : Attribute, IAsyncResourceFilter
    {
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
        /// Gets or sets the name used when mapping the admin route. If empty for the attribute whose <see
        /// cref="Template"/> is used, the fallback is <see cref="ControllerActionDescriptor.DisplayName"/>.
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminAttribute"/> class.
        /// </summary>
        /// <param name="template">If not <see langword="null"/> or empty, it's used as the route template.</param>
        /// <param name="routeName">If not <see langword="null"/> or empty, it's used as the route name.</param>
        /// <param name="nameFromControllerAndAction">
        /// If <see langword="true"/>, the <paramref name="routeName"/> parameter is ignored and <see cref="RouteName"/>
        /// is set to the concatenation of the controller and action names (e.g. the route name for
        /// <c>~/Admin/AdminMenu/List</c> becomes <c>AdminMenuList</c>).
        /// </param>
        public AdminAttribute(string template = null, string routeName = null, bool nameFromControllerAndAction = false)
        {
            Template = template;
            RouteName = nameFromControllerAndAction ? NameFromControllerAndAction : routeName;
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

        public static bool IsApplied(HttpContext context) => context.Items.TryGetValue(typeof(AdminAttribute), out _);
    }
}
