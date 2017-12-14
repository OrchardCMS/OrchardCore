using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Markdown.Drivers;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Handlers;
using OrchardCore.Markdown.Indexing;
using OrchardCore.Markdown.Model;
using OrchardCore.Markdown.Settings;

namespace OrchardCore.Markdown
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Markdown Part
            services.AddScoped<IContentPartDisplayDriver, MarkdownPartDisplay>();
            services.AddSingleton<ContentPart, MarkdownPart>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, MarkdownPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartIndexHandler, MarkdownPartIndexHandler>();
            services.AddScoped<IContentPartHandler, MarkdownPartHandler>();

            // Markdown Field
            services.AddSingleton<ContentField, MarkdownField>();
            services.AddScoped<IContentFieldDisplayDriver, MarkdownFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, MarkdownFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, MarkdownFieldIndexHandler>();
        }
    }
}
