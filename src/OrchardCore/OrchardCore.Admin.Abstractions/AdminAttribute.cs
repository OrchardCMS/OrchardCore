using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.Admin
{
    /// <summary>
    /// When applied to an action or a controller or a page model, intercepts any request to check whether it applies to the admin site.
    /// If so it marks the request as such and ensures the user has the right to access it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AdminAttribute : Attribute, IAsyncResourceFilter
    {
        private static readonly string _adminAttributeItemName = typeof(AdminAttribute).Name;

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

        public bool RequireAccessAdminPanelPermission { get; set; } = true;

        public AdminAttribute(string template = null, string routeName = null, bool requireAccessAdminPanelPermission = true)
        {
            Template = template;
            RouteName = routeName;
            RequireAccessAdminPanelPermission = requireAccessAdminPanelPermission;
        }

        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            Apply(context.HttpContext, this);

            return next();
        }

        public static void Apply(HttpContext context, AdminAttribute attributeValue = null)
        {
            if (context.Items.TryGetValue(_adminAttributeItemName, out var value) && value is AdminAttribute adminAttribute)
            {
                if (attributeValue != null)
                {
                    adminAttribute.RequireAccessAdminPanelPermission = attributeValue.RequireAccessAdminPanelPermission;
                    adminAttribute.Template = attributeValue.Template;
                    adminAttribute.RouteName = attributeValue.RouteName;

                    context.Items[_adminAttributeItemName] = adminAttribute;
                }

                return;
            }

            context.Items[_adminAttributeItemName] = attributeValue ?? new AdminAttribute();
        }

        public static bool IsApplied(HttpContext context)
            => context.Items.ContainsKey(_adminAttributeItemName);

        public static AdminAttribute Get(HttpContext context)
        {
            if (context.Items.TryGetValue(_adminAttributeItemName, out var value) && value is AdminAttribute adminAttribute)
            {
                return adminAttribute;
            }

            return null;
        }
    }
}
