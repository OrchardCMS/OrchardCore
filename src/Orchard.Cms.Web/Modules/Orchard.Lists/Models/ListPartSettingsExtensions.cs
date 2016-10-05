using Orchard.ContentManagement.Metadata.Builders;

namespace Orchard.Lists.Models
{
    public static class ListPartSettingsExtensions
    {
        public static ContentTypePartDefinitionBuilder ContainedContentType(this ContentTypePartDefinitionBuilder builder, string containedContentType)
        {
            return builder.WithSetting("ContainedContentType", containedContentType);
        }
    }
}
