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
        private const string DefaultMarkdownExtensions = "nohtml+advanced";

        private readonly IShellConfiguration _shellConfiguration;

        public Startup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<MarkdownBodyPartViewModel>();
                o.MemberAccessStrategy.Register<MarkdownFieldViewModel>();
            })
            .AddLiquidFilter<Markdownify>("markdownify");

            // Markdown Part
            services.AddContentPart<MarkdownBodyPart>()
                .UseDisplayDriver<MarkdownBodyPartDisplayDriver>()
                .AddHandler<MarkdownBodyPartHandler>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, MarkdownBodyPartSettingsDisplayDriver>();
            services.AddDataMigration<Migrations>();
            services.AddScoped<IContentPartIndexHandler, MarkdownBodyPartIndexHandler>();

            // Markdown Field
            services.AddContentField<MarkdownField>()
                .UseDisplayDriver<MarkdownFieldDisplayDriver>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, MarkdownFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, MarkdownFieldIndexHandler>();

            services.AddOptions<MarkdownPipelineOptions>();
            services.ConfigureMarkdownPipeline((pipeline) =>
            {
                var extensions = _shellConfiguration.GetValue("OrchardCore_Markdown:Extensions", DefaultMarkdownExtensions);
                pipeline.Configure(extensions);
            });

            services.AddScoped<IMarkdownService, DefaultMarkdownService>();
        }
    }
}
