using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapBuilderContext
    {
        public IList<XNamespace> Namespaces { get; set; }
        public XDocument Result { get; set; }
    }
}
