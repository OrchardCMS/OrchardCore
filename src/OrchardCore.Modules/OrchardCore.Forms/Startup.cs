using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
using OrchardCore.Forms.Drivers;
using OrchardCore.Forms.Handlers;
using OrchardCore.Forms.Models;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

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
            TemplateContext.GlobalMemberAccessStrategy.Register<ReCaptchaPart>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<ISite>, ReCaptchaSettingsDisplay>();

            services.AddScoped<IContentPartDisplayDriver, FormPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, FormElementPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, FormInputElementPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, ButtonPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, LabelPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, InputPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, TextAreaPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, ReCaptchaPartDisplay>();

            services.AddSingleton<ContentPart, FormPart>();
            services.AddSingleton<ContentPart, FormElementPart>();
            services.AddSingleton<ContentPart, FormInputElementPart>();
            services.AddSingleton<ContentPart, LabelPart>();
            services.AddSingleton<ContentPart, ButtonPart>();
            services.AddSingleton<ContentPart, InputPart>();
            services.AddSingleton<ContentPart, TextAreaPart>();
            services.AddSingleton<ContentPart, ReCaptchaPart>();

            services.AddScoped<IContentPartHandler, FormInputElementPartHandler>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
        }
    }
}
