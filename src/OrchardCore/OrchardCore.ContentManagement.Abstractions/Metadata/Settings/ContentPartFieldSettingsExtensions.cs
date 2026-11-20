using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement.Metadata.Settings;

public static class ContentPartFieldSettingsExtensions
{
    public static ContentPartFieldDefinitionBuilder WithDisplayName(this ContentPartFieldDefinitionBuilder builder, string displayName)
    {
        return builder.MergeSettings<ContentPartFieldSettings>(x => x.DisplayName = displayName);
    }

    public static ContentPartFieldDefinitionBuilder WithDescription(this ContentPartFieldDefinitionBuilder builder, string description)
    {
        return builder.MergeSettings<ContentPartFieldSettings>(x => x.Description = description);
    }

    public static ContentPartFieldDefinitionBuilder WithEditor(this ContentPartFieldDefinitionBuilder builder, string editor)
    {
        return builder.MergeSettings<ContentPartFieldSettings>(x => x.Editor = editor);
    }

    public static ContentPartFieldDefinitionBuilder WithDisplayMode(this ContentPartFieldDefinitionBuilder builder, string displayMode)
    {
        return builder.MergeSettings<ContentPartFieldSettings>(x => x.DisplayMode = displayMode);
    }

    public static ContentPartFieldDefinitionBuilder WithPosition(this ContentPartFieldDefinitionBuilder builder, string position)
    {
        return builder.MergeSettings<ContentPartFieldSettings>(x => x.Position = position);
    }

    /// <summary>
    /// Sets whether the name of the field is displayed next to its value.
    /// </summary>
    public static ContentPartFieldDefinitionBuilder WithDisplayFieldName(this ContentPartFieldDefinitionBuilder builder, bool displayFieldName = true)
    {
        return builder.MergeSettings<ContentPartFieldSettings>(x => x.DisplayFieldName = displayFieldName);
    }

    /// <summary>
    /// Sets whether the field is displayed when it has no value.
    /// </summary>
    public static ContentPartFieldDefinitionBuilder WithAlwaysDisplay(this ContentPartFieldDefinitionBuilder builder, bool alwaysDisplay = true)
    {
        return builder.MergeSettings<ContentPartFieldSettings>(x => x.AlwaysDisplay = alwaysDisplay);
    }
}
