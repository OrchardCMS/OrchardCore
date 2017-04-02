using Orchard.ContentManagement.Metadata.Builders;

namespace Orchard.Lists.Models
{
    public static class ListPartSettingsExtensions
    {
        public static ContentTypePartDefinitionBuilder ContainedContentTypes(this ContentTypePartDefinitionBuilder builder, string[] containedContentTypes)
        {
            return builder.WithSetting("ContainedContentTypes", containedContentTypes);
        }
    }
}
