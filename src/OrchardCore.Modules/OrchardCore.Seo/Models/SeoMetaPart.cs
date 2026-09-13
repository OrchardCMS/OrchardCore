using System.ComponentModel;
using OrchardCore.ContentManagement;
using OrchardCore.Media.Fields;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Seo.Models;

public class SeoMetaPart : ContentPart
{
    public static readonly char[] InvalidCharactersForCanoncial = "?#[]@!$&'()*+,;=<>\\|%".ToCharArray();

    [Description("The page title used by browsers and search engines.")]
    public string PageTitle { get; set; }

    [DefaultValue(true)]
    [Description("Whether metadata is rendered for the page.")]
    public bool Render { get; set; } = true;

    [Description("The page summary rendered in the description meta tag.")]
    public string MetaDescription { get; set; }

    [Description("The comma-separated keywords rendered in the keywords meta tag.")]
    public string MetaKeywords { get; set; }

    [Description("The canonical URL for the page.")]
    public string Canonical { get; set; }

    [Description("The directives rendered in the robots meta tag.")]
    public string MetaRobots { get; set; }

    [Description("The additional meta tags to render for the page.")]
    public MetaEntry[] CustomMetaTags { get; set; } = [];

    [Description("The default media image used when a network-specific social image is not set.")]
    public MediaField DefaultSocialImage { get; set; }

    [Description("The media image used in Open Graph metadata.")]
    public MediaField OpenGraphImage { get; set; }

    [Description("The Open Graph object type, such as article or website.")]
    public string OpenGraphType { get; set; }

    [Description("The title used in Open Graph metadata.")]
    public string OpenGraphTitle { get; set; }

    [Description("The description used in Open Graph metadata.")]
    public string OpenGraphDescription { get; set; }

    [Description("The media image used in Twitter Card metadata.")]
    public MediaField TwitterImage { get; set; }

    [Description("The title used in Twitter Card metadata.")]
    public string TwitterTitle { get; set; }

    [Description("The description used in Twitter Card metadata.")]
    public string TwitterDescription { get; set; }

    [Description("The Twitter Card type, such as summary or summary_large_image.")]
    public string TwitterCard { get; set; }

    [Description("The Twitter username of the content creator, including the @ prefix.")]
    public string TwitterCreator { get; set; }

    [Description("The Twitter username for the site, including the @ prefix.")]
    public string TwitterSite { get; set; }

    [Description("The JSON-LD structured data rendered for the page.")]
    public string GoogleSchema { get; set; }
}
