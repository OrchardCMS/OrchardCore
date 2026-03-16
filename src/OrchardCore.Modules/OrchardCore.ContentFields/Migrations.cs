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
            await UpgradeAsync();
        }

        // Shortcut other migration steps on new content definition schemas.
        return 3;
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

    // Previously, Liquid rendering was enabled by not having Html sanitization enabled and UpdateFrom1Async disabled
    // sanitization to ensure that HtmlFields kept Liquid rendering enabled. Since Liquid rendering is now controlled
    // by a separate setting, disabling sanitization is no longer necessary.

    public async Task<int> UpdateFrom2Async()
    {
        // To keep the same behavior as before, RenderLiquid is initialized to the opposite of SanitizeHtml.
        var partDefinitions = await _contentDefinitionManager.LoadPartDefinitionsAsync();
        foreach (var partDefinition in partDefinitions)
        {
            if (partDefinition.Fields.Any(f => f.FieldDefinition.Name == "HtmlField"))
            {
                await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
                {
                    foreach (var fieldDefinition in partDefinition.Fields.Where(f => f.FieldDefinition.Name == "HtmlField"))
                    {
                        partBuilder.WithField(fieldDefinition.Name, fieldBuilder =>
                        {
                            fieldBuilder.MergeSettings<HtmlFieldSettings>(s => s.RenderLiquid = !s.SanitizeHtml);
                        });
                    }
                });
            }
        }

        return 3;
    }
}
