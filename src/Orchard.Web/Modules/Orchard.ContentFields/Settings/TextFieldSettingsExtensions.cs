using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.ContentFields.Settings
{
    public static class TextFieldSettingsExtensions
    {
        public static ContentPartFieldDefinitionBuilder Hint(this ContentPartFieldDefinitionBuilder builder, string hint)
        {
            return builder.WithSetting("Hint", hint);
        }
    }
}
