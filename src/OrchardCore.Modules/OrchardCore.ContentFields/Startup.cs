using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Indexing;
using OrchardCore.ContentFields.Services;
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
            TemplateContext.GlobalMemberAccessStrategy.Register<DateField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayDateFieldViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<TimeField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayTimeFieldViewModel>();
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
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldPredefinedListEditorSettingsDriver>();

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

            // Date Field
            services.AddSingleton<ContentField, DateField>();
            services.AddScoped<IContentFieldDisplayDriver, DateFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, DateFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, DateFieldIndexHandler>();

            // Time Field
            services.AddSingleton<ContentField, TimeField>();
            services.AddScoped<IContentFieldDisplayDriver, TimeFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TimeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TimeFieldIndexHandler>();

            // Video field
            services.AddSingleton<ContentField, YoutubeField>();
            services.AddScoped<IContentFieldDisplayDriver, YoutubeFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, YoutubeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, YoutubeFieldIndexHandler>();

            // Content picker field
            services.AddSingleton<ContentField, ContentPickerField>();
            services.AddScoped<IContentFieldDisplayDriver, ContentPickerFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, ContentPickerFieldIndexHandler>();
            services.AddScoped<IContentPickerResultProvider, DefaultContentPickerResultProvider>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "ContentPicker",
                areaName: "OrchardCore.ContentFields",
                template: "ContentPicker",
                defaults: new { controller = "ContentPicker", action = "List" }
            );
        }
    }
}
