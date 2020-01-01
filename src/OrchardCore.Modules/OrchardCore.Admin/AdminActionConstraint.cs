using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace OrchardCore.Admin
{
    public class AdminActionConstraint : IActionConstraint
    {
        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            if (context.RouteContext.HttpContext.Request.Path.Value.StartsWith("/OrchardCore", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
