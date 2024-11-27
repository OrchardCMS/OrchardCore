using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Roles;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRolesCoreServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ISystemRoleNameProvider, DefaultSystemRoleNameProvider>();

        return services;
    }
}
