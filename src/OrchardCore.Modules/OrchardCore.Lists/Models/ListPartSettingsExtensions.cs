using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.Lists.Models
{
    public static class ListPartSettingsExtensions
    {
        public static ContentTypePartDefinitionBuilder ContainedContentTypes(this ContentTypePartDefinitionBuilder builder, string[] containedContentTypes)
        {
            return builder.WithSetting("ContainedContentTypes", containedContentTypes);
        }
    }
}
