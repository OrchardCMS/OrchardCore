using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Deployment;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeploymentTargetHandler<TImplementation>(this IServiceCollection serviceCollection)
        where TImplementation : class, IDeploymentTargetHandler
    {
        serviceCollection.AddScoped<IDeploymentTargetHandler, TImplementation>();

        return serviceCollection;
    }

    public static void AddDeployment<TSource, TStep>(this IServiceCollection services)
        where TSource : IDeploymentSource
        where TStep : DeploymentStep, new()
    {
        services.AddDeploymentSource<TSource>();
        services.AddDeploymentStep<TStep>();
    }

    public static void AddDeployment<TSource, TStep, TDisplayDriver>(this IServiceCollection services)
        where TSource : IDeploymentSource
        where TStep : DeploymentStep, new()
        where TDisplayDriver : DisplayDriver<DeploymentStep, TStep>
    {
        services.AddDeployment<TSource, TStep>();
        services.AddDeploymentStepDisplayDriver<TDisplayDriver, TStep>();
    }

    public static void AddDeploymentWithoutSource<TStep, TDisplayDriver>(this IServiceCollection services)
        where TStep : DeploymentStep, new()
        where TDisplayDriver : DisplayDriver<DeploymentStep, TStep>
    {
        services.AddDeploymentStep<TStep>();
        services.AddDeploymentStepDisplayDriver<TDisplayDriver, TStep>();
    }

    private static void AddDeploymentSource<TSource>(this IServiceCollection services)
        where TSource : IDeploymentSource
    {
        services.AddTransient(typeof(IDeploymentSource), typeof(TSource));
    }

    private static void AddDeploymentStep<TStep>(this IServiceCollection services)
        where TStep : DeploymentStep, new()
    {
        services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<TStep>());
    }

    private static void AddDeploymentStepDisplayDriver<TDisplayDriver, TStep>(this IServiceCollection services)
        where TDisplayDriver : DisplayDriver<DeploymentStep, TStep>
        where TStep : DeploymentStep, new()
    {
        services.AddScoped<IDisplayDriver<DeploymentStep>, TDisplayDriver>();
    }
}
