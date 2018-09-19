using System;
using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Forms.Configuration;
using OrchardCore.Forms.Drivers;
using OrchardCore.Forms.Filters;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.Services;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using Polly;

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
            TemplateContext.GlobalMemberAccessStrategy.Register<NoCaptchaPart>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config =>
            {
                config.Filters.Add<ExportModelStateAttribute>();
                config.Filters.Add<ImportModelStateAttribute>();
            });

            services.AddScoped<IDisplayDriver<ISite>, NoCaptchaSettingsDisplay>();
            services.AddScoped<IContentPartDisplayDriver, FormPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, FormElementPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, FormInputElementPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, ButtonPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, LabelPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, InputPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, TextAreaPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, NoCaptchaPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, ValidationSummaryPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, ValidationPartDisplay>();

            services.AddSingleton<ContentPart, FormPart>();
            services.AddSingleton<ContentPart, FormElementPart>();
            services.AddSingleton<ContentPart, FormInputElementPart>();
            services.AddSingleton<ContentPart, LabelPart>();
            services.AddSingleton<ContentPart, ButtonPart>();
            services.AddSingleton<ContentPart, InputPart>();
            services.AddSingleton<ContentPart, TextAreaPart>();
            services.AddSingleton<ContentPart, NoCaptchaPart>();
            services.AddSingleton<ContentPart, ValidationSummaryPart>();
            services.AddSingleton<ContentPart, ValidationPart>();

            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddTransient<IConfigureOptions<NoCaptchaSettings>, NoCaptchaSettingsConfiguration>();
            services.AddHttpClient<NoCaptchaClient>(client =>
            {
                const string noCaptchaUrl = "https://www.google.com/recaptcha/api/siteverify";
                client.BaseAddress = new Uri(noCaptchaUrl);
            }).AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(0.5 * attempt)));
        }
    }
}
