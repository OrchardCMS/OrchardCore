using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

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
            if (context.RouteContext.HttpContext.Request.Path.StartsWithSegments(_adminUrlPrefix))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
