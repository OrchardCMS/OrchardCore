using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Security.Permissions;

public static class PermissionsServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IPermissionProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPermissionProvider, TProvider>());

        return services;
    }
}
