using System;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Routing;

namespace OrchardCore.Admin
{
    public class AdminAreaControllerRoutePatternProvider : IAreaControllerRoutePatternProvider
    {
        public AdminAreaControllerRoutePatternProvider(IOptions<AdminOptions> adminOptions)
        {
            Pattern = adminOptions.Value.AdminUrlPrefix + "/{area}/{controller}/{action}/{id?}";
        }

        public string Pattern { get; }

        public string ControllerName => "Admin";

        public Type AttributeType => typeof(AdminAttribute);
    }
}
