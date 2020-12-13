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
            TemplateContext.GlobalMemberAccessStrategy.Register<SelectPart>();
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
                    .UseDisplayDriver<FormPartDisplayDriver>();

            services.AddContentPart<FormElementPart>()
                    .UseDisplayDriver<FormElementPartDisplayDriver>();

            services.AddContentPart<FormInputElementPart>()
                    .UseDisplayDriver<FormInputElementPartDisplayDriver>();

            services.AddContentPart<LabelPart>()
                    .UseDisplayDriver<LabelPartDisplayDriver>();

            services.AddContentPart<ButtonPart>()
                    .UseDisplayDriver<ButtonPartDisplayDriver>();

            services.AddContentPart<InputPart>()
                    .UseDisplayDriver<InputPartDisplayDriver>();

            services.AddContentPart<SelectPart>()
                .UseDisplayDriver<SelectPartDisplayDriver>();

            services.AddContentPart<TextAreaPart>()
                    .UseDisplayDriver<TextAreaPartDisplayDriver>();

            services.AddContentPart<ValidationSummaryPart>()
                    .UseDisplayDriver<ValidationSummaryPartDisplayDriver>();

            services.AddContentPart<ValidationPart>()
                    .UseDisplayDriver<ValidationPartDisplayDriver>();

            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
