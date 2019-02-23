using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps
{
    public class SitemapEntryComparer : IEqualityComparer<SitemapEntry>
    {
        public bool Equals(SitemapEntry x, SitemapEntry y)
        {
            return x.Url.Equals(y.Url);
        }

        public int GetHashCode(SitemapEntry obj)
        {
            return obj.Url.GetHashCode();
        }
    }
}
