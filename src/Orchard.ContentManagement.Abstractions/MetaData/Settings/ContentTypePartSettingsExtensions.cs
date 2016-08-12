using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.ContentManagement.Metadata.Settings
{
    public static class ContentTypePartSettingsExtensions
    {
        public static ContentTypePartDefinitionBuilder WithDisplayName(this ContentTypePartDefinitionBuilder builder, string displayName)
        {
            return builder.WithSetting(nameof(ContentTypePartSettings.DisplayName), displayName);
        }

        public static ContentTypePartDefinitionBuilder WithDescription(this ContentTypePartDefinitionBuilder builder, string description)
        {
            return builder.WithSetting(nameof(ContentTypePartSettings.Description), description);
        }
    }
}
