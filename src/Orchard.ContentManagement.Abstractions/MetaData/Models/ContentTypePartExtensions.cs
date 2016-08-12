using System;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Utility;

namespace Orchard.ContentManagement.MetaData.Models
{
    public static class ContentTypePartExtensions
    {
        public static string DisplayName(this ContentTypePartDefinition typePart)
        {
            var displayName = typePart.Settings.ToObject<ContentTypePartSettings>().DisplayName;

            if (String.IsNullOrEmpty(displayName))
            {
                displayName = typePart.PartDefinition.Name.TrimEnd("Part");
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
    }
}
