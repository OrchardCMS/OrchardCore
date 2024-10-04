using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Roles;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRolesCoreServices(this IServiceCollection services)
    {
        return services.AddSingleton<ISystemRoleNameProvider, DefaultSystemRoleNameProvider>();
    }
}
