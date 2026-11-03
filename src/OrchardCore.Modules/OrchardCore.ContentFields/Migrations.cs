using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using Microsoft.Extensions.Logging;

namespace OrchardCore.ContentFields;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ShellDescriptor _shellDescriptor;
    private readonly ILogger _logger;

    public Migrations(
        IContentDefinitionManager contentDefinitionManager,
        ShellDescriptor shellDescriptor,
        ILogger<Migrations> logger)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _shellDescriptor = shellDescriptor;
        _logger = logger;
    }

    // New installations don't need to be upgraded, but because there is no initial migration record,
    // 'UpgradeAsync()' is called in a new 'CreateAsync()' but only if the feature was already installed.
    public async Task<int> CreateAsync()
    {
        if (_shellDescriptor.WasFeatureAlreadyInstalled("OrchardCore.ContentFields"))
        {
            await UpgradeAsync();
            await WarnAboutLiquidTemplatesAsync();
        }

        // Shortcut other migration steps on new content definition schemas.
        return 4;
    }

    // Upgrade an existing installation.
    private async Task UpgradeAsync()
    {
        // Boolean field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<BooleanField, BooleanFieldSettings>();

        // Content picker field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<ContentPickerField, ContentPickerFieldSettings>();

        // Date field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<DateField, DateFieldSettings>();

        // Date time field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<DateTimeField, DateTimeFieldSettings>();

        // Html field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<HtmlField, HtmlFieldSettings>();

        // Link field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<LinkField, LinkFieldSettings>();

        // Localization set content picker field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<LocalizationSetContentPickerField, LocalizationSetContentPickerFieldSettings>();

        // MultiText field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<MultiTextField, MultiTextFieldSettings>();

        // Numeric field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<NumericField, NumericFieldSettings>();

        // Text field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldHeaderDisplaySettings>();
        await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldPredefinedListEditorSettings>();
        await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldSettings>();

        // Time field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<TimeField, TimeFieldSettings>();

        // YouTube field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<YoutubeField, YoutubeFieldSettings>();
    }

    public async Task<int> UpdateFrom2Async()
    {
        await WarnAboutLiquidTemplatesAsync();

        return 4;
    }

    public async Task<int> UpdateFrom3Async()
    {
        await WarnAboutLiquidTemplatesAsync();

        return 4;
    }

    private async Task WarnAboutLiquidTemplatesAsync()
    {
        foreach (var contentType in await _contentDefinitionManager.LoadTypeDefinitionsAsync())
        {
            foreach (var typePart in contentType.Parts)
            {
                foreach (var field in typePart.PartDefinition.Fields.Where(
                    field => string.Equals(field.FieldDefinition.Name, nameof(HtmlField), StringComparison.Ordinal)))
                {
                    if (field.Settings[nameof(HtmlFieldSettings)]?["RenderLiquid"]?.GetValue<bool>() is true)
                    {
                        _logger.LogWarning(
                            "Content type '{ContentType}' part '{Part}' field '{Field}' has RenderLiquid enabled. Liquid syntax remains stored but is no longer executed by HtmlField. Migrate the authored source manually to LiquidField.",
                            contentType.Name,
                            typePart.Name,
                            field.Name);
                    }
                }
            }
        }
    }

}
