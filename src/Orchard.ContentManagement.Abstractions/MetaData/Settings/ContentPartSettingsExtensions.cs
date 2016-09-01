using Orchard.ContentManagement.Metadata.Builders;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.ContentManagement.Metadata.Settings
{
    public static class ContentPartSettingsExtensions
    {
        public static ContentPartDefinitionBuilder Attachable(this ContentPartDefinitionBuilder builder, bool attachable = true)
        {
            return builder.WithSetting(nameof(ContentPartSettings.Attachable), attachable.ToString());
        }

        public static bool IsAttachable(this ContentPartDefinition part)
        {
            return part.Settings.ToObject<ContentPartSettings>().Attachable;
        }

        public static ContentPartDefinitionBuilder Reusable(this ContentPartDefinitionBuilder builder, bool reusable = true)
        {
            return builder.WithSetting(nameof(ContentPartSettings.Reusable), reusable.ToString());
        }

        public static bool IsReusable(this ContentPartDefinition part)
        {
            return part.Settings.ToObject<ContentPartSettings>().Reusable;
        }

        public static ContentPartDefinitionBuilder WithDescription(this ContentPartDefinitionBuilder builder, string description)
        {
            return builder.WithSetting(nameof(ContentPartSettings.Description), description);
        }

        public static string Description(this ContentPartDefinition part)
        {
            return part.Settings.ToObject<ContentPartSettings>().Description;
        }
    }
}
