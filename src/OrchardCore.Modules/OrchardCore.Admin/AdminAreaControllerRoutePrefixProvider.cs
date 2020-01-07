using System;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Routing;

namespace OrchardCore.Admin
{
    public class AdminAreaControllerRoutePrefixProvider : IAreaControllerRoutePrefixProvider
    {
        public AdminAreaControllerRoutePrefixProvider(IOptions<AdminOptions> adminOptions)
        {
            Prefix = adminOptions.Value.AdminUrlPrefix;
        }

        public string Prefix { get; }

        public string ControllerName => "Admin";

        public Type AttributeType => typeof(AdminAttribute);
    }
}
