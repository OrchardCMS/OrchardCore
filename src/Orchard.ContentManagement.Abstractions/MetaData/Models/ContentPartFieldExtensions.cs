using System;
using Orchard.ContentManagement.Metadata.Settings;

namespace Orchard.ContentManagement.Metadata.Models
{
    public static class ContentPartFieldSettingsExtensions
    {
        public static string DisplayName(this ContentPartFieldDefinition partField)
        {
            var displayName = partField.Settings.ToObject<ContentPartFieldSettings>().DisplayName;

            if (String.IsNullOrEmpty(displayName))
            {
                displayName = partField.FieldDefinition.Name;
            }

            return displayName;
        }

        public static string Description(this ContentPartFieldDefinition partField)
        {
            return partField.Settings.ToObject<ContentPartFieldSettings>().Description;
        }
    }
}
