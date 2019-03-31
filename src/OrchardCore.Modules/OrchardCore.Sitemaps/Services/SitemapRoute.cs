using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapRoute : ISitemapRoute
    {
        private readonly IServiceProvider _serviceProvider;
        //keep in memory rather than in MemoryCache for performance?
        private Dictionary<string, string> _routes;

        public SitemapRoute(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> MatchSitemapRouteAsync(string path)
        {
            //don't build until we get a request that might be sitemap related (i.e. .xml)
            if (_routes == null)
            {
                await BuildSitemapRoutes();
            }
            return _routes.ContainsKey(path);
        }

        public async Task<string> GetSitemapNodeByPathAsync(string path)
        {
            //don't build until we get a request that might be sitemap related (i.e. .xml)
            if (_routes == null)
            {
                await BuildSitemapRoutes();
            }
            if (_routes.TryGetValue(path, out string nodeId))
            {
                return nodeId;
            }
            return null;
        }

        public async Task BuildSitemapRoutes(IList<Models.SitemapSet> sitemapSets = null)
        {
            if (sitemapSets == null)
            {
                sitemapSets = await GetSitemapSets();
            }
            _routes = new Dictionary<string, string>();
            foreach (var sitemapSet in sitemapSets.Where(x => x.Enabled))
            {
                var rootPath = sitemapSet.RootPath.TrimStart('/');
                BuildNodeRoutes(sitemapSet.SitemapNodes, rootPath);
            }
        }

        private async Task<IList<SitemapSet>> GetSitemapSets()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var sitemapSetService = scope.ServiceProvider.GetService<ISitemapSetService>();
                return await sitemapSetService.GetAsync();
            }
        }

        private void BuildNodeRoutes(IList<SitemapNode> sitemapNodes, string rootPath)
        {
            foreach (var sitemapNode in sitemapNodes)
            {
                var path = String.Concat(rootPath, sitemapNode.Path);
                _routes.Add(path, sitemapNode.Id);
                if (sitemapNode.ChildNodes != null)
                {
                    BuildNodeRoutes(sitemapNode.ChildNodes, rootPath);
                }
            }
        }
    }
}
