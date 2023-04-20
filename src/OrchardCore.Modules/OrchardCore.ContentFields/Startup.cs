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
using OrchardCore.ContentFields.Handlers;
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
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, BooleanFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, BooleanFieldIndexHandler>();

            // Text Field
            services.AddContentField<TextField>()
                .UseDisplayDriver<TextFieldDisplayDriver>()
                .AddHandler<TextFieldHandler>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TextFieldIndexHandler>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldPredefinedListEditorSettingsDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldMonacoEditorSettingsDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldHeaderDisplaySettingsDriver>();

            // Html Field
            services.AddContentField<HtmlField>()
                .UseDisplayDriver<HtmlFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldSettingsDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldTrumbowygEditorSettingsDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldMonacoEditorSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, HtmlFieldIndexHandler>();

            // Link Field
            services.AddContentField<LinkField>()
                .UseDisplayDriver<LinkFieldDisplayDriver>()
                .AddHandler<LinkFieldHandler>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, LinkFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, LinkFieldIndexHandler>();

            // MultiText Field
            services.AddContentField<MultiTextField>()
                .UseDisplayDriver<MultiTextFieldDisplayDriver>()
                .AddHandler<MultiTextFieldHandler>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, MultiTextFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, MultiTextFieldIndexHandler>();

            // Numeric Field
            services.AddContentField<NumericField>()
                .UseDisplayDriver<NumericFieldDisplayDriver>()
                .AddHandler<NumericFieldHandler>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, NumericFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, NumericFieldIndexHandler>();

            // DateTime Field
            services.AddContentField<DateTimeField>()
                .UseDisplayDriver<DateTimeFieldDisplayDriver>()
                .AddHandler<DateTimeFieldHandler>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, DateTimeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, DateTimeFieldIndexHandler>();

            // Date Field
            services.AddContentField<DateField>()
                .UseDisplayDriver<DateFieldDisplayDriver>()
                .AddHandler<DateFieldHandler>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, DateFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, DateFieldIndexHandler>();

            // Time Field
            services.AddContentField<TimeField>()
                .UseDisplayDriver<TimeFieldDisplayDriver>()
                .AddHandler<TimeFieldHandler>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TimeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TimeFieldIndexHandler>();

            // Video field
            services.AddContentField<YoutubeField>()
                .UseDisplayDriver<YoutubeFieldDisplayDriver>()
                .AddHandler<YoutubeFieldHandler>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, YoutubeFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, YoutubeFieldIndexHandler>();

            // Content picker field
            services.AddContentField<ContentPickerField>()
                .UseDisplayDriver<ContentPickerFieldDisplayDriver>()
                .AddHandler<ContentPickerFieldHandler>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, ContentPickerFieldIndexHandler>();
            services.AddScoped<IContentPickerResultProvider, DefaultContentPickerResultProvider>();

            // Migration, can be removed in a future release.
            services.AddDataMigration<Migrations>();
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
                .UseDisplayDriver<LocalizationSetContentPickerFieldDisplayDriver>()
                .AddHandler<LocalizationSetContentPickerFieldHandler>();

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
    public class IndexingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDataMigration<Indexing.SQL.Migrations>();
            services.AddScopedIndexProvider<TextFieldIndexProvider>();
            services.AddScopedIndexProvider<BooleanFieldIndexProvider>();
            services.AddScopedIndexProvider<NumericFieldIndexProvider>();
            services.AddScopedIndexProvider<DateTimeFieldIndexProvider>();
            services.AddScopedIndexProvider<DateFieldIndexProvider>();
            services.AddScopedIndexProvider<ContentPickerFieldIndexProvider>();
            services.AddScopedIndexProvider<TimeFieldIndexProvider>();
            services.AddScopedIndexProvider<LinkFieldIndexProvider>();
            services.AddScopedIndexProvider<HtmlFieldIndexProvider>();
            services.AddScopedIndexProvider<MultiTextFieldIndexProvider>();
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
                .UseDisplayDriver<UserPickerFieldUserNamesDisplayDriver>(d => String.Equals(d, "UserNames", StringComparison.OrdinalIgnoreCase))
                .AddHandler<UserPickerFieldHandler>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, UserPickerFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, UserPickerFieldIndexHandler>();
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
            services.AddDataMigration<UserPickerMigrations>();
            services.AddScopedIndexProvider<UserPickerFieldIndexProvider>();
        }
    }
}
