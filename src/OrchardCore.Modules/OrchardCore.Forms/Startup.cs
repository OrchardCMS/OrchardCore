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

            services.AddContentPart<FormPart>()
                    .WithDisplayDriver<FormPartDisplay>();

            services.AddContentPart<FormElementPart>()
                    .WithDisplayDriver<FormElementPartDisplay>();

            services.AddContentPart<FormInputElementPart>()
                    .WithDisplayDriver<FormInputElementPartDisplay>();

            services.AddContentPart<LabelPart>()
                    .WithDisplayDriver<LabelPartDisplay>();

            services.AddContentPart<ButtonPart>()
                    .WithDisplayDriver<ButtonPartDisplay>();

            services.AddContentPart<InputPart>()
                    .WithDisplayDriver<InputPartDisplay>();

            services.AddContentPart<TextAreaPart>()
                    .WithDisplayDriver<TextAreaPartDisplay>();

            services.AddContentPart<ValidationSummaryPart>()
                    .WithDisplayDriver<ValidationSummaryPartDisplay>();

            services.AddContentPart<ValidationPart>()
                    .WithDisplayDriver<ValidationPartDisplay>();

            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
