using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapPart : ContentPart
    {
        public ChangeFrequency ChangeFrequency { get; set; }

        public float Priority { get; set; } = 0.5f;

        public bool Exclude { get; set; }

        public SitemapPartSettings Settings { get; set; }
    }
}
