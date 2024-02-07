using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentTypes.Deployment;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.RecipeSteps;
using OrchardCore.ContentTypes.Services;
using OrchardCore.Deployment;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Events;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTypes
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IContentDefinitionService, ContentDefinitionService>();
            services.AddScoped<IStereotypesProvider, DefaultStereotypesProvider>();
            services.AddScoped<IStereotypeService, StereotypeService>();
            services.AddScoped<IContentDefinitionDisplayHandler, ContentDefinitionDisplayCoordinator>();
            services.AddScoped<IContentDefinitionDisplayManager, DefaultContentDefinitionDisplayManager>();
            services.AddScoped<IContentPartDefinitionDisplayDriver, ContentPartSettingsDisplayDriver>();
            services.AddScoped<IContentTypeDefinitionDisplayDriver, ContentTypeSettingsDisplayDriver>();
            services.AddScoped<IContentTypeDefinitionDisplayDriver, DefaultContentTypeDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartSettingsDisplayDriver>();

            // TODO: Put in its own feature to be able to execute this recipe without having to enable
            // Content Types management UI
            services.AddRecipeExecutionStep<ContentDefinitionStep>();
            services.AddRecipeExecutionStep<ReplaceContentDefinitionStep>();
            services.AddRecipeExecutionStep<DeleteContentDefinitionStep>();

            services.AddTransient<IRecipeEventHandler, LuceneRecipeEventHandler>();
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDeployment<ContentDefinitionDeploymentSource, ContentDefinitionDeploymentStep, ContentDefinitionDeploymentStepDriver>();
            services.AddDeployment<ReplaceContentDefinitionDeploymentSource, ReplaceContentDefinitionDeploymentStep, ReplaceContentDefinitionDeploymentStepDriver>();
            services.AddDeployment<DeleteContentDefinitionDeploymentSource, DeleteContentDefinitionDeploymentStep, DeleteContentDefinitionDeploymentStepDriver>();
        }
    }
}
