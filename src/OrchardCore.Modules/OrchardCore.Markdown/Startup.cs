using Fluid;
using Markdig;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Configuration;
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
        private readonly IShellConfiguration _shellConfiguration;

        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<MarkdownBodyPartViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<MarkdownFieldViewModel>();
        }

        public Startup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var markdownOptionsConfig = _shellConfiguration.GetSection("OrchardCore_Markdown");
            var markdownOptions = new MarkdownOptions();
            markdownOptionsConfig.Bind(markdownOptions);

            services.Configure<MarkdownOptions>(markdownOptionsConfig);

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
            services.ConfigureMarkdownPipeline((pipeline) =>
            {
                pipeline.Configure(markdownOptions.Extensions);
            });

            services.AddScoped<IMarkdownService, DefaultMarkdownService>();
        }
    }
}
