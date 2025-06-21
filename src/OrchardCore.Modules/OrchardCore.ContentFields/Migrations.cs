using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.ContentFields;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ShellDescriptor _shellDescriptor;

    public Migrations(
        IContentDefinitionManager contentDefinitionManager,
        ShellDescriptor shellDescriptor)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _shellDescriptor = shellDescriptor;
    }

    // New installations don't need to be upgraded, but because there is no initial migration record,
    // 'UpgradeAsync()' is called in a new 'CreateAsync()' but only if the feature was already installed.
    public async Task<int> CreateAsync()
    {
        if (_shellDescriptor.WasFeatureAlreadyInstalled("OrchardCore.ContentFields"))
        {
            await UpgradeAsync().ConfigureAwait(false);
        }

        // Shortcut other migration steps on new content definition schemas.
        return 2;
    }

    // Upgrade an existing installation.
    private async Task UpgradeAsync()
    {
        // Boolean field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<BooleanField, BooleanFieldSettings>().ConfigureAwait(false);

        // Content picker field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<ContentPickerField, ContentPickerFieldSettings>().ConfigureAwait(false);

        // Date field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<DateField, DateFieldSettings>().ConfigureAwait(false);

        // Date time field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<DateTimeField, DateTimeFieldSettings>().ConfigureAwait(false);

        // Html field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<HtmlField, HtmlFieldSettings>().ConfigureAwait(false);

        // Link field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<LinkField, LinkFieldSettings>().ConfigureAwait(false);

        // Localization set content picker field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<LocalizationSetContentPickerField, LocalizationSetContentPickerFieldSettings>().ConfigureAwait(false);

        // MultiText field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<MultiTextField, MultiTextFieldSettings>().ConfigureAwait(false);

        // Numeric field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<NumericField, NumericFieldSettings>().ConfigureAwait(false);

        // Text field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldHeaderDisplaySettings>().ConfigureAwait(false);
        await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldPredefinedListEditorSettings>().ConfigureAwait(false);
        await _contentDefinitionManager.MigrateFieldSettingsAsync<TextField, TextFieldSettings>().ConfigureAwait(false);

        // Time field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<TimeField, TimeFieldSettings>().ConfigureAwait(false);

        // YouTube field
        await _contentDefinitionManager.MigrateFieldSettingsAsync<YoutubeField, YoutubeFieldSettings>().ConfigureAwait(false);

        // Keep in sync the upgrade process.
        await UpdateFrom1Async().ConfigureAwait(false);
    }

    // This step is part of the upgrade process.
    public async Task<int> UpdateFrom1Async()
    {
        // For backwards compatibility with liquid filters we disable html sanitization on existing field definitions.
        var partDefinitions = await _contentDefinitionManager.LoadPartDefinitionsAsync().ConfigureAwait(false);
        foreach (var partDefinition in partDefinitions)
        {
            if (partDefinition.Fields.Any(x => x.FieldDefinition.Name == "HtmlField"))
            {
                await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
                {
                    foreach (var fieldDefinition in partDefinition.Fields.Where(x => x.FieldDefinition.Name == "HtmlField"))
                    {
                        partBuilder.WithField(fieldDefinition.Name, fieldBuilder =>
                        {
                            fieldBuilder.MergeSettings<HtmlFieldSettings>(x => x.SanitizeHtml = false);
                        });
                    }
                }).ConfigureAwait(false);
            }
        }

        return 2;
    }
}
