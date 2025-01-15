using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Roles;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRolesCoreServices(this IServiceCollection services)
    {
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
        services.TryAddSingleton<ISystemRoleNameProvider, DefaultSystemRoleNameProvider>();
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
        services.TryAddSingleton<ISystemRoleProvider, DefaultSystemRoleProvider>();

        return services;
    }
}
