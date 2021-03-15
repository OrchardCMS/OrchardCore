using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Admin
{
    public class AdminActionConstraint : IActionConstraint
    {
        private readonly PathString _adminUrlPrefix;

        public int Order => 0;

        public AdminActionConstraint(PathString adminUrlPrefix)
        {
            _adminUrlPrefix = adminUrlPrefix;
        }

        public bool Accept(ActionConstraintContext context)
        {
            if (!context.RouteContext.HttpContext.Request.Path.StartsWithSegments(_adminUrlPrefix))
            {
                var logger = context.RouteContext.HttpContext.RequestServices.GetService<ILogger<AdminActionConstraint>>();
                logger.LogWarning("An incorrect admin route is used : {Path}", context.RouteContext.HttpContext.Request.Path);
            }

            return true;
        }
    }
}
