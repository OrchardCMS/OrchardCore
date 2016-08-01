using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.ContentFields.Settings
{
    public static class FieldSettingsExtensions
    {
        public static ContentPartFieldDefinitionBuilder Hint(this ContentPartFieldDefinitionBuilder builder, string hint)
        {
            return builder.WithSetting(nameof(Hint), hint);
        }
    }
}
