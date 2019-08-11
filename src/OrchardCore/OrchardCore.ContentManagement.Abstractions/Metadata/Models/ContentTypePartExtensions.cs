using System;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public static class ContentTypePartExtensions
    {
        public static string DisplayName(this ContentTypePartDefinition typePart)
        {
            var displayName = typePart.Settings.ToObject<ContentTypePartSettings>().DisplayName;

            if (String.IsNullOrEmpty(displayName))
            {
                displayName = typePart.PartDefinition.DisplayName();
            }

            return displayName;
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

        public static string Editor(this ContentTypePartDefinition typePart)
        {
            return typePart.Settings.ToObject<ContentTypePartSettings>().Editor;
        }
    }
}
