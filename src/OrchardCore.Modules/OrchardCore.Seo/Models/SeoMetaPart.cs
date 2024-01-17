using System;
using System.ComponentModel;
using OrchardCore.ContentManagement;
using OrchardCore.Media.Fields;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Seo.Models
{
    public class SeoMetaPart : ContentPart
    {
        public static readonly char[] InvalidCharactersForCanoncial = "?#[]@!$&'()*+,;=<>\\|%".ToCharArray();
        public string PageTitle { get; set; }

        [DefaultValue(true)]
        public bool Render { get; set; } = true;

        public string MetaDescription { get; set; }

        public string MetaKeywords { get; set; }

        public string Canonical { get; set; }

        public string MetaRobots { get; set; }
        public MetaEntry[] CustomMetaTags { get; set; } = Array.Empty<MetaEntry>();


        public MediaField DefaultSocialImage { get; set; }


        public MediaField OpenGraphImage { get; set; }
        public string OpenGraphType { get; set; }
        public string OpenGraphTitle { get; set; }
        public string OpenGraphDescription { get; set; }

        public MediaField TwitterImage { get; set; }
        public string TwitterTitle { get; set; }
        public string TwitterDescription { get; set; }
        public string TwitterCard { get; set; }
        public string TwitterCreator { get; set; }
        public string TwitterSite { get; set; }

        public string GoogleSchema { get; set; }
    }
}
