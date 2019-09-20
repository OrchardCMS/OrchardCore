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
            });

            services.AddScoped<IContentPartDisplayDriver, FormPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, FormElementPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, FormInputElementPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, ButtonPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, LabelPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, InputPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, TextAreaPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, ValidationSummaryPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, ValidationPartDisplay>();

            services.Configure<ContentPartOptions>(options =>
            {
                options.AddPart<FormPart>();
                options.AddPart<FormElementPart>();
                options.AddPart<FormInputElementPart>();
                options.AddPart<LabelPart>();
                options.AddPart<ButtonPart>();
                options.AddPart<InputPart>();
                options.AddPart<TextAreaPart>();
                options.AddPart<ValidationSummaryPart>();
                options.AddPart<ValidationPart>();
            });

            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
