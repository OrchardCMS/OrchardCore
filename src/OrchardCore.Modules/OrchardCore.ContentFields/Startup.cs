using Fluid;
using Microsoft.Extensions.DependencyInjection;
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
using OrchardCore.Liquid;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Users;

namespace OrchardCore.ContentFields;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHtmlServices();

        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
        services.AddScoped<IJSLocalizer, ContentFieldsJSLocalizer>();

        services.Configure<TemplateOptionsBuilder>(o =>
        {
            o.ConfigureMemberAccess(strategy =>
            {
                strategy.Register<BooleanField>();
                strategy.Register<DisplayBooleanFieldViewModel>();
                strategy.Register<HtmlField>();
                strategy.Register<DisplayHtmlFieldViewModel>();
                strategy.Register<LinkField>();
                strategy.Register<DisplayLinkFieldViewModel>();
                strategy.Register<NumericField>();
                strategy.Register<DisplayNumericFieldViewModel>();
                strategy.Register<TextField>();
                strategy.Register<DisplayTextFieldViewModel>();
                strategy.Register<DateTimeField>();
                strategy.Register<DisplayDateTimeFieldViewModel>();
                strategy.Register<DateField>();
                strategy.Register<DisplayDateFieldViewModel>();
                strategy.Register<TimeField>();
                strategy.Register<DisplayTimeFieldViewModel>();
                strategy.Register<MultiTextField>();
                strategy.Register<DisplayMultiTextFieldViewModel>();
                strategy.Register<UserPickerField>();
                strategy.Register<DisplayUserPickerFieldViewModel>();
                strategy.Register<ContentPickerField>();
                strategy.Register<DisplayContentPickerFieldViewModel>();
            });
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
}

[RequireFeatures("OrchardCore.ContentLocalization")]
public sealed class LocalizationSetContentPickerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentField<LocalizationSetContentPickerField>()
            .UseDisplayDriver<LocalizationSetContentPickerFieldDisplayDriver>()
            .AddHandler<LocalizationSetContentPickerFieldHandler>();

        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, LocalizationSetContentPickerFieldSettingsDriver>();
        services.AddScoped<IContentFieldIndexHandler, LocalizationSetContentPickerFieldIndexHandler>();
    }
}

[Feature("OrchardCore.ContentFields.Indexing.SQL")]
public sealed class IndexingStartup : StartupBase
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

[RequireFeatures(UserConstants.Features.Users)]
public sealed class UserPickerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptionsBuilder>(o =>
        {
            o.ConfigureMemberAccess(strategy =>
            {
                strategy.Register<UserPickerField>();
                strategy.Register<DisplayUserPickerFieldViewModel>();
                strategy.Register<DisplayUserPickerFieldUserNamesViewModel>();
            });
        });

        services.AddContentField<UserPickerField>()
            .UseDisplayDriver<UserPickerFieldDisplayDriver>(d => !string.Equals(d, "UserNames", StringComparison.OrdinalIgnoreCase))
            .UseDisplayDriver<UserPickerFieldUserNamesDisplayDriver>(d => string.Equals(d, "UserNames", StringComparison.OrdinalIgnoreCase))
            .AddHandler<UserPickerFieldHandler>();

        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, UserPickerFieldSettingsDriver>();
        services.AddScoped<IContentFieldIndexHandler, UserPickerFieldIndexHandler>();
        services.AddScoped<IUserPickerResultProvider, DefaultUserPickerResultProvider>();
    }
}

[Feature("OrchardCore.ContentFields.Indexing.SQL.UserPicker")]
public sealed class UserPickerSqlIndexingStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<UserPickerMigrations>();
        services.AddScopedIndexProvider<UserPickerFieldIndexProvider>();
    }
}
