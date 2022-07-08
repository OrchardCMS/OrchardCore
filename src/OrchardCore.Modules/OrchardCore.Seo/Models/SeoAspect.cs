using System;
using System.Globalization;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Seo.Models
{
    public class SeoAspect
    {
        public string PageTitle { get; set; }
        public bool Render { get; set; } = true;

        public string MetaDescription { get; set; }

        public string MetaKeywords { get; set; }
        public string Canonical { get; set; }

        public string MetaRobots { get; set; }
        public MetaEntry[] CustomMetaTags { get; set; } = Array.Empty<MetaEntry>();

        // Twitter card
        public string TwitterCard { get; set; }
        public string TwitterSite { get; set; } // comes from settings.
        public string TwitterCreator { get; set; }
        public string TwitterTitle { get; set; } // comes from page title.
        public string TwitterDescription { get; set; } // comes from MetaDescription
        public string TwitterUrl { get; set; }

        public string TwitterImage { get; set; }
        public string TwitterImageAlt { get; set; }

        // OpenGraph
        public string OpenGraphType { get; set; }
        public string OpenGraphTitle { get; set; } // comes from page title
        public string OpenGraphSiteName { get; set; }
        public string OpenGraphDescription { get; set; } // comes from MetaDescription
        public string OpenGraphUrl { get; set; }
        public string OpenGraphImage { get; set; }
        public string OpenGraphImageAlt { get; set; }
        public string OpenGraphLocale { get; set; } = CultureInfo.CurrentUICulture.Name;
        public string OpenGraphAppId { get; set; }

        public string GoogleSchema { get; set; }
    }
}
