using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.ContentManagement.Metadata.Settings
{
    public static class ContentPartSettingsExtensions
    {
        public static ContentPartDefinitionBuilder Attachable(this ContentPartDefinitionBuilder builder, bool attachable = true)
        {
            return builder.WithSetting("Attachable", attachable.ToString());
        }

        public static ContentPartDefinitionBuilder WithDescription(this ContentPartDefinitionBuilder builder, string description)
        {
            return builder.WithSetting("Description", description);
        }
    }
}
