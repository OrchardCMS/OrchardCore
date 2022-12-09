using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Roles.Services;
using OrchardCore.Security.Services;
using OrchardCore.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class RolesServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the roles services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        public static IServiceCollection AddRoles(this IServiceCollection services)
        {
            services.TryAddScoped<RoleManager<IRole>>();
            services.TryAddScoped<IRoleStore<IRole>, RoleStore>();
            services.TryAddScoped<IRoleService, RoleService>();
            services.TryAddScoped<IRoleClaimStore<IRole>, RoleStore>();

            return services;
        }
    }
}
