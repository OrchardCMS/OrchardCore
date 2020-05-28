using Fluid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Forms.Drivers;
using OrchardCore.Forms.Filters;
using OrchardCore.Forms.Models;
using OrchardCore.Modules;

namespace OrchardCore.Forms
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<FormPart>();
            TemplateContext.GlobalMemberAccessStrategy.Register<FormElementPart>();
            TemplateContext.GlobalMemberAccessStrategy.Register<FormInputElementPart>();
            TemplateContext.GlobalMemberAccessStrategy.Register<LabelPart>();
            TemplateContext.GlobalMemberAccessStrategy.Register<InputPart>();
            TemplateContext.GlobalMemberAccessStrategy.Register<TextAreaPart>();
            TemplateContext.GlobalMemberAccessStrategy.Register<ButtonPart>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<ExportModelStateAttribute>();
                options.Filters.Add<ImportModelStateAttribute>();
                options.Filters.Add<ImportModelStatePageFilter>();
            });

            services.AddScoped<IContentDisplayDriver, FormContentDisplayDriver>();

            services.AddContentPart<FormPart>()
                    .UseDisplayDriver<FormPartDisplay>();

            services.AddContentPart<FormElementPart>()
                    .UseDisplayDriver<FormElementPartDisplay>();

            services.AddContentPart<FormInputElementPart>()
                    .UseDisplayDriver<FormInputElementPartDisplay>();

            services.AddContentPart<LabelPart>()
                    .UseDisplayDriver<LabelPartDisplay>();

            services.AddContentPart<ButtonPart>()
                    .UseDisplayDriver<ButtonPartDisplay>();

            services.AddContentPart<InputPart>()
                    .UseDisplayDriver<InputPartDisplay>();

            services.AddContentPart<TextAreaPart>()
                    .UseDisplayDriver<TextAreaPartDisplay>();

            services.AddContentPart<ValidationSummaryPart>()
                    .UseDisplayDriver<ValidationSummaryPartDisplay>();

            services.AddContentPart<ValidationPart>()
                    .UseDisplayDriver<ValidationPartDisplay>();

            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
