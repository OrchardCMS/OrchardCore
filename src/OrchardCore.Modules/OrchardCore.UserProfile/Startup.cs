using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.UserProfile
{
    public class Startup : StartupBase
    {
        private readonly string _tenantName;
        private readonly string _tenantPrefix;

        public Startup(ShellSettings shellSettings)
        {
            _tenantName = shellSettings.Name;
            _tenantPrefix = "/" + shellSettings.RequestUrlPrefix;
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {

        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentDefinitionManager, ContentDefinitionManager>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}