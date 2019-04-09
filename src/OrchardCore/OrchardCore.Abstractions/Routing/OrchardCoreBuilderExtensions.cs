using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using OrchardCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds a bridge between the global routing system and a tenant route constraint.
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
                    // Prevents duplicate keys.
                    options.ConstraintMap.Remove(key);
                });
            },

            // Before anybody.
            order: int.MinValue);
        }

        /// <summary>
        /// Adds a bridge between the global routing system and a tenant endpoint selector policy.
        /// </summary>
        public static OrchardCoreBuilder AddEndpointSelectorPolicy<T>(this OrchardCoreBuilder builder) where T : MatcherPolicy, IEndpointSelectorPolicy
        {
            return builder.AddEndpointSelectorPolicy(typeof(T));
        }

        /// <summary>
        /// Adds a bridge between the global routing system and a tenant node builder policy.
        /// </summary>
        public static OrchardCoreBuilder AddNodeBuilderPolicy<T>(this OrchardCoreBuilder builder) where T : MatcherPolicy, IEndpointComparerPolicy, INodeBuilderPolicy
        {
            return builder.AddNodeBuilderPolicy(typeof(T));
        }

        /// <summary>
        /// Adds a bridge between the global routing system and a tenant endpoint selector policy.
        /// </summary>
        public static OrchardCoreBuilder AddEndpointSelectorPolicy(this OrchardCoreBuilder builder, Type type)
        {
            builder.ApplicationServices.AddSingleton<MatcherPolicy>(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                return new ShellEndpointSelectorPolicy(httpContextAccessor, type);
            });

            return builder;
        }

        /// <summary>
        /// Adds a bridge between the global routing system and a tenant node builder policy.
        /// </summary>
        public static OrchardCoreBuilder AddNodeBuilderPolicy(this OrchardCoreBuilder builder, Type type)
        {
            builder.ApplicationServices.AddSingleton<MatcherPolicy>(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                return new ShellNodeBuilderPolicy(httpContextAccessor, type);
            });

            return builder;
        }
    }
}
