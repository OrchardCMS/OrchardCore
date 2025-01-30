using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Security.Permissions;

public static class PermissionsServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IPermissionProvider
    {
        if (!services.Any(s => s.ImplementationType == typeof(TProvider)))
        {
            services.AddScoped<IPermissionProvider, TProvider>();
        }

        return services;
    }
}
