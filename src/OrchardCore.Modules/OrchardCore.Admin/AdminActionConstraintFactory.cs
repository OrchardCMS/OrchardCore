using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Admin
{
    public class AdminActionConstraintFactory : IActionConstraintFactory
    {
        public bool IsReusable => true;

        public IActionConstraint CreateInstance(IServiceProvider services)
        {
            var adminOptions = services.GetRequiredService<IOptions<AdminOptions>>();
            return new AdminActionConstraint(new PathString('/' + adminOptions.Value.AdminUrlPrefix.TrimStart('/')));
        }
    }
}
