using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Indexing;
using OrchardCore.ContentFields.Indexing.SQL;
using OrchardCore.ContentFields.Services;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
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
            services.AddContentField<BooleanField>();
            services.AddScoped<IContentFieldDisplayDriver, BooleanFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, BooleanFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, BooleanFieldIndexHandler>();

            // Text Field
            services.AddContentField<TextField>();
            services.AddScoped<IContentFieldDisplayDriver, TextFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TextFieldIndexHandler>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldPredefinedListEditorSettingsDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldHeaderDisplaySettingsDriver>();

            // Html Field
            services.AddContentField<HtmlField>();
            services.AddScoped<IContentFieldDisplayDriver, HtmlFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldSettingsDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldTrumbowygEditorSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, HtmlFieldIndexHandler>();

            // Link Field
            services.AddContentField<LinkField>();
            services.AddScoped<IContentFieldDisplayDriver, LinkFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, LinkFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, LinkFieldIndexHandler>();

            // Numeric Field
            services.AddContentField<NumericField>();
            services.AddScoped<IContentFieldDisplayDriver, NumericFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, NumericFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, NumericFieldIndexHandler>();

            // DateTime Field
            services.AddContentField<DateTimeField>();
            services.AddScoped<IContentFieldDisplayDriver, DateTimeFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, DateTimeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, DateTimeFieldIndexHandler>();

            // Date Field
            services.AddContentField<DateField>();
            services.AddScoped<IContentFieldDisplayDriver, DateFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, DateFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, DateFieldIndexHandler>();

            // Time Field
            services.AddContentField<TimeField>();
            services.AddScoped<IContentFieldDisplayDriver, TimeFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TimeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TimeFieldIndexHandler>();

            // Video field
            services.AddContentField<YoutubeField>();
            services.AddScoped<IContentFieldDisplayDriver, YoutubeFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, YoutubeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, YoutubeFieldIndexHandler>();

            // Content picker field
            services.AddContentField<ContentPickerField>();
            services.AddScoped<IContentFieldDisplayDriver, ContentPickerFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, ContentPickerFieldIndexHandler>();
            services.AddScoped<IContentPickerResultProvider, DefaultContentPickerResultProvider>();

            // Migration, can be removed in a future release.
            services.AddScoped<IDataMigration, Migrations>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "ContentPicker",
                areaName: "OrchardCore.ContentFields",
                pattern: "ContentFields/SearchContentItems",
                defaults: new { controller = "ContentPickerAdmin", action = "SearchContentItems" }
            );
        }
    }

    [RequireFeatures("OrchardCore.ContentLocalization")]
    public class LocalizationSetContentPickerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentField<LocalizationSetContentPickerField>();
            services.AddScoped<IContentFieldDisplayDriver, LocalizationSetContentPickerFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, LocalizationSetContentPickerFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, LocalizationSetContentPickerFieldIndexHandler>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "SearchLocalizationSets",
                areaName: "OrchardCore.ContentFields",
                pattern: "ContentFields/SearchLocalizationSets",
                defaults: new { controller = "LocalizationSetContentPickerAdmin", action = "SearchLocalizationSets" }
            );
        }
    }

    [Feature("OrchardCore.ContentFields.Indexing.SQL")]
    [RequireFeatures("OrchardCore.ContentFields")]
    public class IndexingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Indexing.SQL.Migrations>();
            services.AddScoped<IScopedIndexProvider, TextFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, BooleanFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, NumericFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, DateTimeFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, DateFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, ContentPickerFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, TimeFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, LinkFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, HtmlFieldIndexProvider>();
        }
    }
}
