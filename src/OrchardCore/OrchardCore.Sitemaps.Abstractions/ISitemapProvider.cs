using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps
{
    public interface ISitemapProvider
    {
        Task<IList<SitemapEntry>> BuildSitemapEntries();
    }
}
