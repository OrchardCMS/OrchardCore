using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Deployment.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddDeployment<TSource, TStep>(this IServiceCollection services)
        where TSource : IDeploymentSource
        where TStep : DeploymentStep, new()
    {
        services.AddTransient<IDeploymentSource, IDeploymentSource>();
        services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<TStep>());
    }

    public static void AddDeployment<TSource, TStep, TDisplayDriver>(this IServiceCollection services)
        where TSource : IDeploymentSource
        where TStep : DeploymentStep, new()
        where TDisplayDriver : DisplayDriver<DeploymentStep, TStep>
    {
        services.AddDeployment<TSource, TStep>();
        services.AddScoped<IDisplayDriver<DeploymentStep>, TDisplayDriver>();
    }

    public static void AddDeploymentWithoutSource<TStep, TDisplayDriver>(this IServiceCollection services)
        where TStep : DeploymentStep, new()
        where TDisplayDriver : DisplayDriver<DeploymentStep, TStep>
    {
        services.AddScoped<IDisplayDriver<DeploymentStep>, TDisplayDriver>();
        services.AddScoped<IDisplayDriver<DeploymentStep>, TDisplayDriver>();
    }
}
