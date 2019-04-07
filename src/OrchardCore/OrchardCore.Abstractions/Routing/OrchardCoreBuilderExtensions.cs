using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using OrchardCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds a global route constraint which can be applied in a tenant context.
        /// </summary>
        public static OrchardCoreBuilder AddRouteConstraint<T>(this OrchardCoreBuilder builder, string key) where T : class, IRouteConstraint
        {
            builder.ApplicationServices.Configure<RouteOptions>(options =>
            {
                // Adds the constraint at the host level.
                options.ConstraintMap.Add(key, typeof(ShellRouteConstraint<T>));
            });

            return builder.ConfigureServices(services =>
            {
                // Registers the related tenant service.
                services.AddSingleton<IRouteConstraint, T>();

                services.Configure<RouteOptions>(options =>
                {
                    // A constraint added at the tenant level is not taken into account.
                    // So, we remove it just to prevent from failing on a duplicate key.
                    options.ConstraintMap.Remove(key);
                });
            },

            // Before anybody.
            order: -10000);
        }

        /// <summary>
        /// Adds a global endpoint selector policy which can be applied in a tenant context.
        /// </summary>
        public static OrchardCoreBuilder AddEndpointSelectorPolicy<T>(this OrchardCoreBuilder builder) where T : MatcherPolicy, IEndpointSelectorPolicy
        {
            builder.ApplicationServices.AddSingleton<MatcherPolicy>(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                return new ShellEndpointSelectorPolicy<T>(httpContextAccessor);
            });

            return builder;
        }

        /// <summary>
        /// Adds a global endpoint selector policy which can be applied in a tenant context.
        /// </summary>
        public static OrchardCoreBuilder AddEndpointSelectorPolicy(this OrchardCoreBuilder builder, string typeFullName)
        {
            builder.ApplicationServices.AddSingleton<MatcherPolicy>(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                return new ShellEndpointSelectorPolicy(httpContextAccessor, typeFullName);
            });

            return builder;
        }

        /// <summary>
        /// Adds a global endpoint selector policy which can be applied in a tenant context.
        /// </summary>
        public static OrchardCoreBuilder AddNodeBuilderPolicy<T>(this OrchardCoreBuilder builder) where T : MatcherPolicy, IEndpointComparerPolicy, INodeBuilderPolicy
        {
            builder.ApplicationServices.AddSingleton<MatcherPolicy>(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                return new ShellNodeBuilderPolicy<T>(httpContextAccessor);
            });

            return builder;
        }

        /// <summary>
        /// Adds a global endpoint selector policy which can be applied in a tenant context.
        /// </summary>
        public static OrchardCoreBuilder AddNodeBuilderPolicy(this OrchardCoreBuilder builder, string typeFullName)
        {
            builder.ApplicationServices.AddSingleton<MatcherPolicy>(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                return new ShellNodeBuilderPolicy(httpContextAccessor, typeFullName);
            });

            return builder;
        }
    }
}
