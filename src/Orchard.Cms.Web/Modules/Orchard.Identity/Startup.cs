using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migration;
using Orchard.Identity.Handlers;
using Orchard.Identity.Indexes;
using Orchard.Identity.Models;
using YesSql.Core.Indexes;

namespace Orchard.Identity
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IIndexProvider, IdentityPartIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();

            // Identity Part
            services.AddScoped<ContentPart, IdentityPart>();
            services.AddScoped<IContentPartHandler, IdentityPartHandler>();
        }
    }
}
