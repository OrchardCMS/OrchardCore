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
        
        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<BooleanField>();
                o.MemberAccessStrategy.Register<DisplayBooleanFieldViewModel>();
                o.MemberAccessStrategy.Register<HtmlField>();
                o.MemberAccessStrategy.Register<DisplayHtmlFieldViewModel>();
                o.MemberAccessStrategy.Register<LinkField>();
                o.MemberAccessStrategy.Register<DisplayLinkFieldViewModel>();
                o.MemberAccessStrategy.Register<NumericField>();
                o.MemberAccessStrategy.Register<DisplayNumericFieldViewModel>();
                o.MemberAccessStrategy.Register<TextField>();
                o.MemberAccessStrategy.Register<DisplayTextFieldViewModel>();
                o.MemberAccessStrategy.Register<DateTimeField>();
                o.MemberAccessStrategy.Register<DisplayDateTimeFieldViewModel>();
                o.MemberAccessStrategy.Register<DateField>();
                o.MemberAccessStrategy.Register<DisplayDateFieldViewModel>();
                o.MemberAccessStrategy.Register<TimeField>();
                o.MemberAccessStrategy.Register<DisplayTimeFieldViewModel>();
                o.MemberAccessStrategy.Register<MultiTextField>();
                o.MemberAccessStrategy.Register<DisplayMultiTextFieldViewModel>();
                o.MemberAccessStrategy.Register<UserPickerField>();
                o.MemberAccessStrategy.Register<DisplayUserPickerFieldViewModel>();
                o.MemberAccessStrategy.Register<ContentPickerField>();
                o.MemberAccessStrategy.Register<DisplayContentPickerFieldViewModel>();
            });

            // Boolean Field
            services.AddContentField<BooleanField>()
                .UseDisplayDriver<BooleanFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, BooleanFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, BooleanFieldIndexHandler>();

            // Text Field
            services.AddContentField<TextField>()
                .UseDisplayDriver<TextFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, TextFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, TextFieldIndexHandler>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, TextFieldPredefinedListEditorSettingsDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, TextFieldMonacoEditorSettingsDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, TextFieldHeaderDisplaySettingsDriver>();

            // Html Field
            services.AddContentField<HtmlField>()
                .UseDisplayDriver<HtmlFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, HtmlFieldSettingsDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, HtmlFieldTrumbowygEditorSettingsDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, HtmlFieldMonacoEditorSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, HtmlFieldIndexHandler>();

            // Link Field
            services.AddContentField<LinkField>()
                .UseDisplayDriver<LinkFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, LinkFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, LinkFieldIndexHandler>();

            // MultiText Field
            services.AddContentField<MultiTextField>()
                .UseDisplayDriver<MultiTextFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, MultiTextFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, MultiTextFieldIndexHandler>();

            // Numeric Field
            services.AddContentField<NumericField>()
                .UseDisplayDriver<NumericFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, NumericFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, NumericFieldIndexHandler>();

            // DateTime Field
            services.AddContentField<DateTimeField>()
                .UseDisplayDriver<DateTimeFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, DateTimeFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, DateTimeFieldIndexHandler>();

            // Date Field
            services.AddContentField<DateField>()
                .UseDisplayDriver<DateFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, DateFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, DateFieldIndexHandler>();

            // Time Field
            services.AddContentField<TimeField>()
                .UseDisplayDriver<TimeFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, TimeFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, TimeFieldIndexHandler>();

            // Video field
            services.AddContentField<YoutubeField>()
                .UseDisplayDriver<YoutubeFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, YoutubeFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, YoutubeFieldIndexHandler>();

            // Content picker field
            services.AddContentField<ContentPickerField>()
                .UseDisplayDriver<ContentPickerFieldDisplayDriver>();
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, ContentPickerFieldIndexHandler>();
            services.AddScoped<IContentPickerResultProvider, DefaultContentPickerResultProvider>();

            // Migration, can be removed in a future release.
            services.AddTransient<IDataMigration, Migrations>();
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
            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, LocalizationSetContentPickerFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, LocalizationSetContentPickerFieldIndexHandler>();
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
    public class IndexingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDataMigration, Indexing.SQL.Migrations>();
            services.AddScoped<IScopedIndexProvider, TextFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, BooleanFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, NumericFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, DateTimeFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, DateFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, ContentPickerFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, TimeFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, LinkFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, HtmlFieldIndexProvider>();
            services.AddScoped<IScopedIndexProvider, MultiTextFieldIndexProvider>();
        }
    }

    [RequireFeatures("OrchardCore.Users")]
    public class UserPickerStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public UserPickerStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<UserPickerField>();
                o.MemberAccessStrategy.Register<DisplayUserPickerFieldViewModel>();
                o.MemberAccessStrategy.Register<DisplayUserPickerFieldUserNamesViewModel>();
            });

            services.AddContentField<UserPickerField>()
                .UseDisplayDriver<UserPickerFieldDisplayDriver>(d => !String.Equals(d, "UserNames", StringComparison.OrdinalIgnoreCase))
                .UseDisplayDriver<UserPickerFieldUserNamesDisplayDriver>(d => String.Equals(d, "UserNames", StringComparison.OrdinalIgnoreCase));

            services.AddTransient<IContentPartFieldDefinitionDisplayDriver, UserPickerFieldSettingsDriver>();
            services.AddTransient<IContentFieldIndexHandler, UserPickerFieldIndexHandler>();
            services.AddScoped<IUserPickerResultProvider, DefaultUserPickerResultProvider>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "SearchUsers",
                areaName: "OrchardCore.ContentFields",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentFields/SearchUsers",
                defaults: new { controller = typeof(UserPickerAdminController).ControllerName(), action = nameof(UserPickerAdminController.SearchUsers) }
            );
        }
    }

    [Feature("OrchardCore.ContentFields.Indexing.SQL.UserPicker")]
    public class UserPickerSqlIndexingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDataMigration, Indexing.SQL.UserPickerMigrations>();
            services.AddScoped<IScopedIndexProvider, UserPickerFieldIndexProvider>();
        }
    }
}
