using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapsSettings
    {
        /// <summary>
        /// Adjust this setting (defaults to 50,000) if sitemaps have exceeded 10MB (uncompressed) in size
        /// </summary>
        public int MaxEntriesPerSitemap { get; set; }
    }
}
