using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Recipes.Executors;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes
{
    public static class ModularServicesBuilderExtensions
    {
        public static ModularServicesBuilder AddRecipeFeatures(this ModularServicesBuilder builder, params string[] featureIds)
        {
            builder.Services.ConfigureTenantServices((collection) =>
            {
                collection.AddScoped<IRecipeStepHandler>(sp => new AddRecipeFeaturesStep(featureIds));
            });

            return builder;
        }
        public static ModularServicesBuilder RemoveRecipeFeatures(this ModularServicesBuilder builder, params string[] featureIds)
        {
            builder.Services.ConfigureTenantServices((collection) =>
            {
                collection.AddScoped<IRecipeStepHandler>(sp => new RemoveRecipeFeaturesStep(featureIds));
            });

            return builder;
        }
    }
}
