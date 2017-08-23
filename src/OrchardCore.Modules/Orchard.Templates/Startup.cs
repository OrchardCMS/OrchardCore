using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement;
using Orchard.Environment.Navigation;
using Orchard.Recipes;
using Orchard.Security.Permissions;
using Orchard.Templates.Recipes;
using Orchard.Templates.Services;
using Orchard.Templates.Settings;

namespace Orchard.Templates
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShapeBindingResolver, TemplatesShapeBindingResolver>();
            services.AddScoped<PreviewTemplatesProvider>();
            services.AddScoped<TemplatesManager>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddRecipeExecutionStep<TemplateStep>();

            // Template shortcuts in settings
            services.AddScoped<IContentPartDefinitionDisplayDriver, TemplateContentPartSettingsDriver>();
            services.AddScoped<IContentTypeDefinitionDisplayDriver, TemplateContentTypeSettingsDriver>();
        }
    }
}
