using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
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
            // Host level route endpoint selector policy.
            builder.ApplicationServices.AddSingleton<MatcherPolicy, FormValueRequiredMatcherPolicy>();

            // Host level endpoint scheme allowing a tenant to add its own schemes for link generation.
            builder.ApplicationServices.AddSingleton<IEndpointAddressScheme<RouteValuesAddress>, ShellRouteValuesAddressScheme>();

            // The global routing system is not aware of tenant level constraints and policies.
            // So, we need bridges to expose them but still instantiate them in a tenant scope.

            builder.AddRouteConstraint<KnownRouteValueConstraint>("exists");

            // Auto discover internal mvc policies.
            var policyTypes = GetMvcPolicyTypes();

            // Add tenant mvc policy bridges.
            foreach (var type in policyTypes)
            {
                if (typeof(IEndpointSelectorPolicy).IsAssignableFrom(type))
                {
                    builder.AddEndpointSelectorPolicy(type);
                }

                else if (typeof(INodeBuilderPolicy).IsAssignableFrom(type))
                {
                    builder.AddNodeBuilderPolicy(type);
                }
            }

            return builder.RegisterStartup<Startup>();
        }

        private static IEnumerable<Type> GetMvcPolicyTypes()
        {
            var services = new ServiceCollection()
                .AddSingleton(new ApplicationPartManager())
                .AddRouting();

            var hostRoutingServicesCount = services.Count;

            return services.AddMvcCore().AddRazorPages()
                .Services.Skip(hostRoutingServicesCount)
                .Where(sd => sd.ServiceType == typeof(MatcherPolicy))
                .Select(sd => sd.ImplementationType);
        }
    }
}
