using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Mvc;
using OrchardCore.Mvc.Routing;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level MVC services and configuration.
        /// </summary>
        public static OrchardCoreBuilder AddMvc(this OrchardCoreBuilder builder)
        {
            builder.ApplicationServices.AddSingleton<MatcherPolicy, FormValueRequiredMatcherPolicy>();

            builder.AddRouteConstraint<KnownRouteValueConstraint>("exists");
            builder.AddEndpointSelectorPolicy("Microsoft.AspNetCore.Mvc.Routing.ActionConstraintMatcherPolicy");
            builder.AddEndpointSelectorPolicy("Microsoft.AspNetCore.Mvc.Routing.DynamicControllerEndpointMatcherPolicy");
            builder.AddNodeBuilderPolicy("Microsoft.AspNetCore.Mvc.Routing.ConsumesMatcherPolicy");

            return builder.RegisterStartup<Startup>();
        }
    }
}
