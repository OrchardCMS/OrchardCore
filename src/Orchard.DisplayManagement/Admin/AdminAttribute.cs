using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Microsoft.AspNetCore.Http;

namespace Orchard.DisplayManagement.Admin
{
    /// <summary>
    /// Intercepts any request to check whether it applies to the admin site.
    /// If so it marks the request as such and ensures the user as the right to access it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AdminAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext context)
        {
            Apply(context.HttpContext);
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Apply(context.HttpContext);

            // TODO: Check permission
            //if (!_authorizer.Authorize(StandardPermissions.AccessAdminPanel, T("Can't access the admin")))
            //{
            //    filterContext.Result = new HttpUnauthorizedResult();
            //}
        }

        public static void Apply(HttpContext context)
        {
            // The value isn't important, it's just a marker object
            context.Items[typeof(AdminAttribute)] = null;
        }

        public static bool IsApplied(HttpContext context)
        {
            object value;
            return context.Items.TryGetValue(typeof(AdminAttribute), out value);
        }

        /// <summary>
        /// Returns <c>true</c> if the controller name starts with Admin or if the <see cref="AdminAttribute"/>
        /// is applied to the controller or the action.
        /// </summary>
        private static bool IsAdmin(ActionExecutingContext context)
        {
            // Does the controller start with "Admin"
            if (IsNameAdmin(context))
            {
                return true;
            }
            
            return false;
        }

        private static bool IsNameAdmin(ActionExecutingContext context)
        {
            return string.Equals(context.Controller.GetType().Name, "Admin",
                                 StringComparison.OrdinalIgnoreCase);
        }
    }
}
