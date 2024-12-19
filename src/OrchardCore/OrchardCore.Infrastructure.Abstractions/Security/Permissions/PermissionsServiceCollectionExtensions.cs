using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Security.Permissions;

public static class PermissionsServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IPermissionProvider
    {
        return services.AddScoped<IPermissionProvider, TProvider>();
    }
}
