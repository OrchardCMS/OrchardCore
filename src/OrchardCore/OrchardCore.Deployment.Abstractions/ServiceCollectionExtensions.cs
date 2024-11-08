using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Deployment;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeploymentTargetHandler<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IDeploymentTargetHandler
    {
        services.AddScoped<IDeploymentTargetHandler, TImplementation>();

        return services;
    }

    public static IServiceCollection AddDeployment<TSource, TStep>(this IServiceCollection services)
        where TSource : IDeploymentSource
        where TStep : DeploymentStep, new()
    {
        services.AddDeploymentSource<TSource>()
            .AddDeploymentStep<TStep>();

        return services;
    }

    public static IServiceCollection AddDeployment<TSource, TStep, TDisplayDriver>(this IServiceCollection services)
        where TSource : IDeploymentSource
        where TStep : DeploymentStep, new()
        where TDisplayDriver : DisplayDriver<DeploymentStep, TStep>
    {
        services.AddDeployment<TSource, TStep>()
            .AddDeploymentStepDisplayDriver<TDisplayDriver, TStep>();

        return services;
    }

    public static IServiceCollection AddDeploymentWithoutSource<TStep, TDisplayDriver>(this IServiceCollection services)
        where TStep : DeploymentStep, new()
        where TDisplayDriver : DisplayDriver<DeploymentStep, TStep>
    {
        services.AddDeploymentStep<TStep>()
            .AddDeploymentStepDisplayDriver<TDisplayDriver, TStep>();

        return services;
    }

    private static IServiceCollection AddDeploymentSource<TSource>(this IServiceCollection services)
        where TSource : IDeploymentSource
    {
        services.AddTransient(typeof(IDeploymentSource), typeof(TSource));

        return services;
    }

    private static IServiceCollection AddDeploymentStep<TStep>(this IServiceCollection services)
        where TStep : DeploymentStep, new()
    {
        services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<TStep>());

        // TStep instances are part for DeploymentPlan objects that are serialized to JSON
        services.AddJsonDerivedTypeInfo<TStep, DeploymentStep>();

        return services;
    }

    private static IServiceCollection AddDeploymentStepDisplayDriver<TDisplayDriver, TStep>(this IServiceCollection services)
        where TDisplayDriver : DisplayDriver<DeploymentStep, TStep>
        where TStep : DeploymentStep, new()
    {
        services.AddDisplayDriver<DeploymentStep, TDisplayDriver>();

        return services;
    }
}
