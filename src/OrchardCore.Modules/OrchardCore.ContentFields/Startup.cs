using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Indexing;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            // Registering both field types and shape types are necessary as they can 
            // be accessed from inner properties.
            
            TemplateContext.GlobalMemberAccessStrategy.Register<BooleanField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayBooleanFieldViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<HtmlField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayHtmlFieldViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<LinkField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayLinkFieldViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<NumericField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayNumericFieldViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<TextField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayTextFieldViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DateTimeField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayDateTimeFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Boolean Field
            services.AddSingleton<ContentField, BooleanField>();
            services.AddScoped<IContentFieldDisplayDriver, BooleanFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, BooleanFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, BooleanFieldIndexHandler>();

            // Text Field
            services.AddSingleton<ContentField, TextField>();
            services.AddScoped<IContentFieldDisplayDriver, TextFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TextFieldIndexHandler>();

            // Html Field
            services.AddSingleton<ContentField, HtmlField>();
            services.AddScoped<IContentFieldDisplayDriver, HtmlFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, HtmlFieldIndexHandler>();

            // Link Field
            services.AddSingleton<ContentField, LinkField>();
            services.AddScoped<IContentFieldDisplayDriver, LinkFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, LinkFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, LinkFieldIndexHandler>();

            // Numeric Field
            services.AddSingleton<ContentField, NumericField>();
            services.AddScoped<IContentFieldDisplayDriver, NumericFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, NumericFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, NumericFieldIndexHandler>();

            // DateTime Field
            services.AddSingleton<ContentField, DateTimeField>();
            services.AddScoped<IContentFieldDisplayDriver, DateTimeFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, DateTimeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, DateTimeFieldIndexHandler>();
        }
    }
}
