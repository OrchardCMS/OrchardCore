using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.Lists.Models
{
    public static class ListPartSettingsExtensions
    {
        public static ContentTypePartDefinitionBuilder ClearContainedContentTypes(this ContentTypePartDefinitionBuilder builder)
        {
            return builder.WithSetting("ContainedContentTypes", new string[] { });
        }

        public static ContentTypePartDefinitionBuilder ContainedContentTypes(this ContentTypePartDefinitionBuilder builder, string[] containedContentTypes, bool keepExistingTypes = true)
        {
            return keepExistingTypes
                ? builder.MergeSettings(new JObject() { ["ContainedContentTypes"] = new JArray(containedContentTypes) })
                : builder.WithSetting("ContainedContentTypes", containedContentTypes);
        }
    }
}
