using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapRoute
    {
        Task BuildSitemapRoutes(IList<SitemapSet> sitemapSets = null);
        Task<string> GetSitemapNodeByPathAsync(string path);
        Task<bool> MatchSitemapRouteAsync(string path);
    }
}