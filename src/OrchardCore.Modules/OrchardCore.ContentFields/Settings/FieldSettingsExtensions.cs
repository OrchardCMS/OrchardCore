using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentFields.Settings
{
    public static class FieldSettingsExtensions
    {
        public static ContentPartFieldDefinitionBuilder Hint(this ContentPartFieldDefinitionBuilder builder, string hint)
        {
            return builder.WithSetting(nameof(Hint), hint);
        }
    }
}
