using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentTypes.Editors;
using Orchard.Environment.Navigation;
using Orchard.Indexing.Services;
using Orchard.Indexing.Settings;
using Orchard.Tokens;

namespace Orchard.Indexing
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentItemIndexHandler, ContentItemIndexCoordinator>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentIndexSettingsDisplayDriver>();
            services.AddSingleton(new IndexManager());
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IndexingTaskManager>();

            services.AddNullTokenizer();
        }
    }
}
