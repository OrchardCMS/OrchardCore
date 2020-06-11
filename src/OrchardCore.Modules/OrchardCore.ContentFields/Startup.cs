using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentFields.Controllers;
using OrchardCore.ContentFields.Drivers;
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
using OrchardCore.Mvc.Core.Utilities;

namespace OrchardCore.ContentFields
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;
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

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Boolean Field
            services.AddContentField<BooleanField>()
                .UseDisplayDriver<BooleanFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, BooleanFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, BooleanFieldIndexHandler>();

            // Text Field
            services.AddContentField<TextField>()
                .UseDisplayDriver<TextFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TextFieldIndexHandler>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldPredefinedListEditorSettingsDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldHeaderDisplaySettingsDriver>();

            // Html Field
            services.AddContentField<HtmlField>()
                .UseDisplayDriver<HtmlFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldSettingsDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldTrumbowygEditorSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, HtmlFieldIndexHandler>();

            // Link Field
            services.AddContentField<LinkField>()
                .UseDisplayDriver<LinkFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, LinkFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, LinkFieldIndexHandler>();

            // Numeric Field
            services.AddContentField<NumericField>()
                .UseDisplayDriver<NumericFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, NumericFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, NumericFieldIndexHandler>();

            // DateTime Field
            services.AddContentField<DateTimeField>()
                .UseDisplayDriver<DateTimeFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, DateTimeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, DateTimeFieldIndexHandler>();

            // Date Field
            services.AddContentField<DateField>()
                .UseDisplayDriver<DateFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, DateFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, DateFieldIndexHandler>();

            // Time Field
            services.AddContentField<TimeField>()
                .UseDisplayDriver<TimeFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TimeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TimeFieldIndexHandler>();

            // Video field
            services.AddContentField<YoutubeField>()
                .UseDisplayDriver<YoutubeFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, YoutubeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, YoutubeFieldIndexHandler>();

            // Content picker field
            services.AddContentField<ContentPickerField>()
                .UseDisplayDriver<ContentPickerFieldDisplayDriver>();
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
                pattern: _adminOptions.AdminUrlPrefix + "/ContentFields/SearchContentItems",
                defaults: new { controller = typeof(ContentPickerAdminController).ControllerName(), action = nameof(ContentPickerAdminController.SearchContentItems) }
            );
        }
    }

    [RequireFeatures("OrchardCore.ContentLocalization")]
    public class LocalizationSetContentPickerStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public LocalizationSetContentPickerStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentField<LocalizationSetContentPickerField>()
                .UseDisplayDriver<LocalizationSetContentPickerFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, LocalizationSetContentPickerFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, LocalizationSetContentPickerFieldIndexHandler>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "SearchLocalizationSets",
                areaName: "OrchardCore.ContentFields",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentFields/SearchLocalizationSets",
                defaults: new { controller = typeof(LocalizationSetContentPickerAdminController).ControllerName(), action = nameof(LocalizationSetContentPickerAdminController.SearchLocalizationSets) }
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
