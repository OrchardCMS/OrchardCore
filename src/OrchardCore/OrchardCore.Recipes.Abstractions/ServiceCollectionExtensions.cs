using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRecipeExecutionStep<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IRecipeStepHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRecipeStepHandler, TImplementation>());

        return services;
    }

    /// <summary>
    /// Registers a recipe file of the extension whose startup is calling this method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The recipe is only available while the feature registering it is enabled. A setup recipe has to be registered
    /// by a feature of the setup shell, for instance a feature added with <c>AddSetupFeatures()</c>, to be listed
    /// by the setup screens.
    /// </para>
    /// <para>
    /// The file must not be located directly in the <c>Recipes</c> folder of the extension, which is still scanned
    /// for recipes regardless of the enabled features. Use a sub folder instead, e.g. <c>Recipes/Setup/blog.recipe.json</c>.
    /// </para>
    /// <para>
    /// When called outside of a feature startup, for instance on the host service collection, the path is relative to
    /// the content root of the application.
    /// </para>
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="path">The path of the recipe file, relative to the root of the extension.</param>
    public static IServiceCollection AddRecipe(this IServiceCollection services, string path)
    {
        var feature = (services as IFeatureAwareServiceCollection)?.CurrentFeature;

        return services.AddRecipe(path, feature?.Extension?.Id);
    }

    /// <summary>
    /// Registers a recipe file of a given extension.
    /// </summary>
    /// <remarks>
    /// The recipe is only available while the feature registering it is enabled, which allows a feature to expose
    /// the recipes of another extension, for instance the setup recipes of a theme.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="path">The path of the recipe file, relative to the root of the extension.</param>
    /// <param name="extensionId">The identifier of the extension holding the recipe file, or <see langword="null"/> for a file of the application content root.</param>
    public static IServiceCollection AddRecipe(this IServiceCollection services, string path, string extensionId)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var featureId = (services as IFeatureAwareServiceCollection)?.CurrentFeature?.Id;

        services.Configure<RecipeOptions>(options => options.Recipes.Add(new RecipeRegistration(path, extensionId, featureId)));

        return services;
    }
}
