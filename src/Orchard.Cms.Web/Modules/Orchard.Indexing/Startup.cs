using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migration;
using Orchard.Indexing.Services;
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
            services.AddScoped<IIndexingTaskManager, IndexingTaskManager>();
            services.AddScoped<IContentHandler, CreateIndexingTaskContentHandler>();
            services.AddScoped<IDataMigration, Migrations>();

            services.AddNullTokenizer();
        }
    }
}
