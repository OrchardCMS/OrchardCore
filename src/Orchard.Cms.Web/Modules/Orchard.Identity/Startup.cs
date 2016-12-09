using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentTypes.Editors;
using Orchard.Data.Migration;
using Orchard.Identity.Drivers;
using Orchard.Identity.Handlers;
using Orchard.Identity.Indexes;
using Orchard.Identity.Indexing;
using Orchard.Identity.Models;
using Orchard.Identity.Services;
using Orchard.Identity.Settings;
using Orchard.Indexing;
using YesSql.Core.Indexes;

namespace Orchard.Identity
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IIndexProvider, IdentityPartIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentIdentityProvider, IdentityProvider>();

            // Identity Part
            services.AddScoped<IContentPartDisplayDriver, IdentityPartDisplay>();
            services.AddScoped<ContentPart, IdentityPart>();
            services.AddScoped<IContentPartHandler, IdentityPartHandler>();
            services.AddScoped<IContentPartIndexHandler, IdentityPartIndexHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, IdentityPartSettingsDisplayDriver>();

        }
    }
}
