using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Metadata.Models;
using OrchardCore.Metadata.Fields;
using OrchardCore.Metadata.Settings;

namespace OrchardCore.Metadata
{
    public class Migrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(SeoMetadataPart), part => part
                .Attachable()
                .Reusable(false)
                .WithDisplayName("SEO Metadata")
                .WithDescription("Provides metadata for search engines to your content items.")
                .WithField(nameof(SeoMetadataPart.PageTitle), field => field
                    .OfType(nameof(MetadataTextField))
                    .WithDisplayName("Page title")
                    .WithSettings(new MetadataTextFieldSettings
                    {
                        MaxCharacterLength = 60,
                        Hint = "Use the page title field when it is necessary to modify the content" +
                        " item's title to improve search engine optimisaton.",
                        DescriptorAttibuteType = AttributeType.notApplicable
                    }))
                .WithField(nameof(SeoMetadataPart.MetaDescription), field => field
                    .OfType(nameof(MetadataTextField))
                    .WithDisplayName("Meta description")
                    .WithEditor("TextArea")
                    .WithSettings(new MetadataTextFieldSettings
                    {
                        MaxCharacterLength = 160,
                        Hint = "The page description or 'meta description' provides a brief summary" +
                        " of a web page. It is sometimes used by search engines in search results.",
                        DescriptorAttibuteType = AttributeType.name,
                        Descriptor = "description"
                    }))
                .WithField(nameof(SeoMetadataPart.MetaKeywords), field => field
                    .OfType(nameof(MetadataTextField))
                    .WithDisplayName("Meta keywords")
                    .WithEditor("TextArea")
                    .WithSettings(new MetadataTextFieldSettings
                    {
                        MaxCharacterLength = 160,
                        Hint = "A short list of keywords separated by commas that are associated" +
                        " with the content item. Some search engines,such as Google, no longer use" +
                        " the meta keywords to influence search results.",
                        DescriptorAttibuteType = AttributeType.name,
                        Descriptor = "keywords"
                    }))
                );

            _contentDefinitionManager.AlterPartDefinition(nameof(SocialMetadataPart), part => part
                .Attachable()
                .Reusable(false)
                .WithDescription("Provides metadata for social networks to your content items.")
                .WithField(nameof(SocialMetadataPart.OpenGraphTitle), field => field
                    .OfType(nameof(MetadataTextField))
                    .WithDisplayName("Open Graph title (og:title)")
                    .WithSettings(new MetadataTextFieldSettings
                    {
                        MaxCharacterLength = 150,
                        Hint = "Used when sharing the content on social networks using the Open Graph title tag.",
                        DescriptorAttibuteType = AttributeType.property,
                        Descriptor = "og:title"
                    }))
                .WithField(nameof(SocialMetadataPart.OpenGraphDescription), field => field
                    .OfType(nameof(MetadataTextField))
                    .WithDisplayName("Open Graph description (og:description)")
                    .WithSettings(new MetadataTextFieldSettings
                    {
                        MaxCharacterLength = 150,
                        Hint = "Used when sharing the content on social networks using the Open Graph description tag.",
                        DescriptorAttibuteType = AttributeType.property,
                        Descriptor = "og:description"
                    }))
                .WithField(nameof(SocialMetadataPart.OpenGraphType), field => field
                    .OfType(nameof(MetadataTextField))
                    .WithDisplayName("Open Graph type (og:type)")
                    .WithSettings(new MetadataTextFieldSettings
                    {
                        DefaultValue = "website",
                        DescriptorAttibuteType = AttributeType.property,
                        Descriptor = "og:type"
                    }))
                .WithField(nameof(SocialMetadataPart.TwitterCard), field => field
                    .OfType(nameof(MetadataTextField))
                    .WithDisplayName("Twitter Cards (twitter:card)")
                    .WithSettings(new MetadataTextFieldSettings
                    {
                        DefaultValue = "summary_large_image",
                        DescriptorAttibuteType = AttributeType.name,
                        Descriptor = "twitter:card"
                    }))
                );

            return 1;
        }
    }
}
