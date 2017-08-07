using Orchard.ContentManagement.Metadata.Builders;

namespace Orchard.Flows.Models
{
    public static class BagPartSettingsExtensions
    {
        public static ContentTypePartDefinitionBuilder ContainedContentTypes(this ContentTypePartDefinitionBuilder builder, string[] containedContentTypes)
        {
            return builder.WithSetting("ContainedContentTypes", containedContentTypes);
        }
    }
}
