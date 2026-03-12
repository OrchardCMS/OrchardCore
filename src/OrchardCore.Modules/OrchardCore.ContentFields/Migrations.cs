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
        return 2;
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
}
