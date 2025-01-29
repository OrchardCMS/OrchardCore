using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.AuditTrail.Drivers;
using OrchardCore.Contents.AuditTrail.Handlers;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.Services;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;

namespace OrchardCore.Contents.AuditTrail;

[RequireFeatures("OrchardCore.AuditTrail")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<Migrations>();
        services.AddContentPart<AuditTrailPart>()
            .UseDisplayDriver<AuditTrailPartDisplayDriver>();

        services.AddScoped<IContentTypePartDefinitionDisplayDriver, AuditTrailPartSettingsDisplayDriver>();
        services.AddScoped<IContentDisplayDriver, AuditTrailContentsDriver>();

        services.AddTransient<IConfigureOptions<AuditTrailOptions>, ContentAuditTrailEventConfiguration>();
        services.AddScoped<IAuditTrailEventHandler, ContentAuditTrailEventHandler>();
        services.AddSiteDisplayDriver<ContentAuditTrailSettingsDisplayDriver>();

        services.AddDisplayDriver<AuditTrailEvent, AuditTrailContentEventDisplayDriver>();

        services.AddScoped<IContentHandler, AuditTrailContentHandler>();
    }
}
