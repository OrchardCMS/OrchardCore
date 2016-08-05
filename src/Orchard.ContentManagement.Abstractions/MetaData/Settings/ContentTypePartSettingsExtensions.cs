using System;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Utility;

namespace Orchard.ContentManagement.Metadata.Settings
{
    public static class ContentTypePartSettingsExtensions
    {
        public static ContentTypePartDefinitionBuilder WithDisplayName(this ContentTypePartDefinitionBuilder builder, string displayName)
        {
            return builder.WithSetting(nameof(ContentTypePartSettings.DisplayName), displayName);
        }

        public static string DisplayName(this ContentTypePartDefinition typePart)
        {
            var displayName = typePart.Settings.ToObject<ContentTypePartSettings>().DisplayName;

            if (String.IsNullOrEmpty(displayName))
            {
                displayName = typePart.PartDefinition.Name.TrimEnd("Part");
            }

            return displayName;
        }

        public static ContentTypePartDefinitionBuilder WithDescription(this ContentTypePartDefinitionBuilder builder, string description)
        {
            return builder.WithSetting(nameof(ContentTypePartSettings.Description), description);
        }

        public static string Description(this ContentTypePartDefinition typePart)
        {
            var description = typePart.Settings.ToObject<ContentTypePartSettings>().Description;

            if (String.IsNullOrEmpty(description))
            {
                description = typePart.PartDefinition.Description();
            }

            return description;
        }
    }
}
