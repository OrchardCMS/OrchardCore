using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
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
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IDeploymentSource), typeof(TSource)));

        return services;
    }

    private static IServiceCollection AddDeploymentStep<TStep>(this IServiceCollection services)
        where TStep : DeploymentStep, new()
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDeploymentStepFactory>(new DeploymentStepFactory<TStep>()));

        // TStep instances are part for DeploymentPlan objects that are serialized to JSON
        services.AddJsonDerivedTypeInfo<TStep, DeploymentStep>();

        return services;
    }

    /// <summary>
    /// Configures display metadata (title, description) for an auto-discovered recipe-based deployment step.
    /// This is optional — without it, the deployment step uses the recipe step name as the title
    /// and an auto-generated description.
    /// </summary>
    /// <typeparam name="TLocalizer">The type used for string localization.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="recipeStepName">The recipe step name (must match <see cref="OrchardCore.Recipes.Services.IRecipeDeploymentStep.Name"/>).</param>
    /// <param name="title">A factory function that produces the display title.</param>
    /// <param name="description">A factory function that produces the display description.</param>
    public static IServiceCollection AddRecipeStepDeployment<TLocalizer>(
        this IServiceCollection services,
        string recipeStepName,
        Func<IStringLocalizer, LocalizedString> title,
        Func<IStringLocalizer, string> description)
        where TLocalizer : class
    {
        // Register display metadata using a service provider callback for localization.
        services.AddTransient<IConfigureOptions<RecipeStepDeploymentOptions>>(sp =>
        {
            var stringLocalizer = sp.GetService<IStringLocalizer<TLocalizer>>();
            return new ConfigureNamedOptions<RecipeStepDeploymentOptions>(null, options =>
            {
                options.Steps[recipeStepName] = new RecipeStepDeploymentInfo
                {
                    Title = title(stringLocalizer),
                    Description = description(stringLocalizer),
                };
            });
        });

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
