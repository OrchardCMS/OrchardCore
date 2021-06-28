using System;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public static class ContentTypePartExtensions
    {
        public static string DisplayName(this ContentTypePartDefinition typePart)
        {
            var displayName = typePart.GetSettings<ContentTypePartSettings>().DisplayName;

            if (String.IsNullOrEmpty(displayName))
            {
                // ContentType creates a same named ContentPart. As DisplayName is not stored in ContentPart,
                // fetching it from the parent ContentType
                if (typePart.PartDefinition.Name == typePart.ContentTypeDefinition.Name)
                {
                    displayName = typePart.ContentTypeDefinition.DisplayName;
                }
                else
                    displayName = typePart.PartDefinition.DisplayName();
            }

            return displayName;
        }

        public static string Description(this ContentTypePartDefinition typePart)
        {
            var description = typePart.GetSettings<ContentTypePartSettings>().Description;

            if (String.IsNullOrEmpty(description))
            {
                description = typePart.PartDefinition.Description();
            }

            return description;
        }

        public static string Editor(this ContentTypePartDefinition typePart)
        {
            return typePart.GetSettings<ContentTypePartSettings>().Editor;
        }

        public static string DisplayMode(this ContentTypePartDefinition typePart)
        {
            return typePart.GetSettings<ContentTypePartSettings>().DisplayMode;
        }
    }
}
