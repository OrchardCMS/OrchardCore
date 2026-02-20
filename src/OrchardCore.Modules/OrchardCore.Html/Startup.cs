using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Html.Drivers;
using OrchardCore.Html.Fields;
using OrchardCore.Html.Handlers;
using OrchardCore.Html.Indexing;
using OrchardCore.Html.Indexing.SQL;
using OrchardCore.Html.Models;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.Html;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<DisplayHtmlFieldViewModel>();
            o.MemberAccessStrategy.Register<HtmlField>();
            o.MemberAccessStrategy.Register<HtmlBodyPartViewModel>();
        });

        // Html Field
        services.AddContentField<HtmlField>()
            .UseDisplayDriver<HtmlFieldDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldSettingsDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldTrumbowygEditorSettingsDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldMonacoEditorSettingsDriver>();
        services.AddScoped<IContentFieldIndexHandler, HtmlFieldIndexHandler>();

        // Body Part
        services.AddContentPart<HtmlBodyPart>()
            .UseDisplayDriver<HtmlBodyPartDisplayDriver>()
            .AddHandler<HtmlBodyPartHandler>();

        services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlBodyPartSettingsDisplayDriver>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlBodyPartTrumbowygEditorSettingsDriver>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlBodyPartMonacoEditorSettingsDriver>();
        services.AddDataMigration<Migrations>();
        services.AddScoped<IContentPartIndexHandler, HtmlBodyPartIndexHandler>();
    }

    [Feature("OrchardCore.Html.Indexing.SQL")]
    public sealed class IndexingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDataMigration<Indexing.SQL.Migrations>();
            services.AddScopedIndexProvider<HtmlFieldIndexProvider>();
        }
    }
}
