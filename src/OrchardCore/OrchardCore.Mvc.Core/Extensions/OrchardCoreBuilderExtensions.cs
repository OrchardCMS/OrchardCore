using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Mvc;
using OrchardCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level MVC services and configuration.
        /// </summary>
        public static OrchardCoreBuilder AddMvc(this OrchardCoreBuilder builder)
        {
            // Host level routing policy.
            builder.ApplicationServices.AddSingleton<MatcherPolicy, FormValueRequiredMatcherPolicy>();

            // Host level scheme allowing a given tenant to add its own schemes for link generation.
            builder.ApplicationServices.AddSingleton<IEndpointAddressScheme<RouteValuesAddress>, ShellRouteValuesAddressScheme>();

            // The global endpoint routing system is not aware of tenant level constraints and policies.
            // So, we need wrappers so that there are found and then still executed in a tenant context.

            builder.AddRouteConstraint<KnownRouteValueConstraint>("exists");
            builder.AddEndpointSelectorPolicy("Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.PageLoaderMatcherPolicy");
            builder.AddEndpointSelectorPolicy("Microsoft.AspNetCore.Mvc.Routing.ActionConstraintMatcherPolicy");
            builder.AddEndpointSelectorPolicy("Microsoft.AspNetCore.Mvc.Routing.DynamicControllerEndpointMatcherPolicy");
            builder.AddNodeBuilderPolicy("Microsoft.AspNetCore.Mvc.Routing.ConsumesMatcherPolicy");

            return builder.RegisterStartup<Startup>();
        }
    }
}
