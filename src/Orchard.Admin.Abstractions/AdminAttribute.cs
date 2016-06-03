using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Orchard.Admin
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    /// <summary>
    /// When applied to an action or a controller, intercepts any request to check whether it applies to the admin site.
    /// If so it marks the request as such and ensures the user as the right to access it.
    /// </summary>
    public class AdminAttribute : ServiceFilterAttribute
    {
        public AdminAttribute() : base(typeof(AdminFilter))
        {
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
            return string.Equals(context.Controller.GetType().Name, "Admin", StringComparison.OrdinalIgnoreCase);
        }

    }

    /// <summary>
    /// Intercepts any request to check whether it applies to the admin site.
    /// If so it marks the request as such and ensures the user as the right to access it.
    /// </summary>
    public class AdminFilter : IAsyncAuthorizationFilter
    {
        private readonly IAuthorizationService _authorizationService;

        public AdminFilter(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            AdminAttribute.Apply(context.HttpContext);

            var authorized = await _authorizationService.AuthorizeAsync(context.HttpContext.User, Permissions.AccessAdminPanel);

            if (!authorized)
            {
                context.Result = new UnauthorizedResult();
            }
        }

    }
}
