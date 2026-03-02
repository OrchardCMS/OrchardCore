using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentTypes.Deployment;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.RecipeSteps;
using OrchardCore.ContentTypes.Services;
using OrchardCore.Deployment;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Events;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTypes;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ContentTypesOptions>(options =>
        {
            var configSection = _shellConfiguration.GetSection("OrchardCore_ContentTypes");

            var defaultThumbnailPath = configSection.GetValue<string>("DefaultThumbnailPath");
            if (!string.IsNullOrWhiteSpace(defaultThumbnailPath))
            {
                options.DefaultThumbnailPath = defaultThumbnailPath;
            }
            else
            {
                options.DefaultThumbnailPath = "~/OrchardCore.ContentTypes/placeholder.png";
            }

            options.DefaultCategory = configSection.GetValue<string>("DefaultCategory");
        });
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddScoped<IContentDefinitionService, ContentDefinitionService>();
        services.AddScoped<IStereotypeService, StereotypeService>();
        services.AddScoped<IContentDefinitionDisplayHandler, ContentDefinitionDisplayCoordinator>();
        services.AddScoped<IContentDefinitionDisplayManager, DefaultContentDefinitionDisplayManager>();
        services.AddScoped<IContentPartDefinitionDisplayDriver, ContentPartSettingsDisplayDriver>();
        services.AddScoped<IContentTypeDefinitionDisplayDriver, ContentTypeSettingsDisplayDriver>();
        services.AddScoped<IContentTypeDefinitionDisplayDriver, DefaultContentTypeDisplayDriver>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartSettingsDisplayDriver>();

        // Register the unified step for content definitions.
        services.AddRecipeDeploymentStep<UnifiedContentDefinitionStep>();
        services.AddRecipeDeploymentStep<ReplaceContentDefinitionRecipeStep>();
        services.AddRecipeDeploymentStep<DeleteContentDefinitionRecipeStep>();
#pragma warning disable CS0618 // Type or member is obsolete - kept for backwards compatibility
        services.AddRecipeExecutionStep<ReplaceContentDefinitionStep>();
        services.AddRecipeExecutionStep<DeleteContentDefinitionStep>();
#pragma warning restore CS0618
        services.AddTransient<IRecipeEventHandler, LuceneRecipeEventHandler>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<ContentDefinitionDeploymentSource, ContentDefinitionDeploymentStep, ContentDefinitionDeploymentStepDriver>();
        services.AddDeployment<ReplaceContentDefinitionDeploymentSource, ReplaceContentDefinitionDeploymentStep, ReplaceContentDefinitionDeploymentStepDriver>();
        services.AddDeployment<DeleteContentDefinitionDeploymentSource, DeleteContentDefinitionDeploymentStep, DeleteContentDefinitionDeploymentStepDriver>();
    }
}
