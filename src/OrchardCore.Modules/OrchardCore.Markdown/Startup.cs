using Fluid;
using Markdig;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Drivers;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Filters;
using OrchardCore.Markdown.Handlers;
using OrchardCore.Markdown.Indexing;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Markdown
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<MarkdownBodyPartViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<MarkdownFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Markdown Part
            services.AddContentPart<MarkdownBodyPart>()
                .UseDisplayDriver<MarkdownBodyPartDisplay>()
                .AddHandler<MarkdownBodyPartHandler>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, MarkdownBodyPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartIndexHandler, MarkdownBodyPartIndexHandler>();

            // Markdown Field
            services.AddContentField<MarkdownField>()
                .UseDisplayDriver<MarkdownFieldDisplayDriver>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, MarkdownFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, MarkdownFieldIndexHandler>();

            services.AddLiquidFilter<Markdownify>("markdownify");

            services.AddOptions<MarkdownPipelineOptions>();
            services.ConfigureMarkdownPipeline((pipeline) => pipeline.DisableHtml());

            services.AddScoped<IMarkdownService, DefaultMarkdownService>();
        }
    }
}
