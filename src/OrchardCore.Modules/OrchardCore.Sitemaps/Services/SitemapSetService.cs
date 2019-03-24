using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapSetService : ISitemapSetService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IServiceProvider _serviceProvider;
        private const string SitemapSetCacheKey = "SitemapSetService";
        //keep in memory rather than in MemoryCache for performance
        private HashSet<string> _routes;


        public SitemapSetService(
            ISignal signal,
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache)
        {
            _signal = signal;
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
        }

        public IChangeToken ChangeToken => _signal.GetToken(SitemapSetCacheKey);

        public async Task<bool> MatchSitemapRouteAsync(string path)
        {
            //don't build until we get a request that might be sitemap related (i.e. .xml)
            if (_routes == null)
            {
                await BuildSitemapRoutes();
            }
            return _routes.Contains(path);
        }

        public async Task<List<Models.SitemapSet>> GetAsync()
        {
            return (await GetSitemapSetList()).SitemapSets;
        }

        public async Task SaveAsync(Models.SitemapSet tree)
        {
            var sitemapSetList = await GetSitemapSetList();
            var session = GetSession();

            var preExisting = sitemapSetList.SitemapSets.Where(x => x.Id == tree.Id).FirstOrDefault();

            // it's new? add it
            if (preExisting == null)
            {
                sitemapSetList.SitemapSets.Add(tree);
            }
            else // not new: replace it
            {
                var index = sitemapSetList.SitemapSets.IndexOf(preExisting);
                sitemapSetList.SitemapSets[index] = tree;
            }

            session.Save(sitemapSetList);
            await BuildSitemapRoutes(sitemapSetList);
            _memoryCache.Set(SitemapSetCacheKey, sitemapSetList);
            _signal.SignalToken(SitemapSetCacheKey);
        }

        public async Task<Models.SitemapSet> GetByIdAsync(string id)
        {
            return (await GetSitemapSetList())
                .SitemapSets
                .Where(x => String.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }


        public async Task<int> DeleteAsync(Models.SitemapSet tree)
        {
            var sitemapSetList = await GetSitemapSetList();
            var session = GetSession();

            var count = sitemapSetList.SitemapSets.RemoveAll(x => String.Equals(x.Id, tree.Id));

            session.Save(sitemapSetList);
            await BuildSitemapRoutes(sitemapSetList);
            _memoryCache.Set(SitemapSetCacheKey, sitemapSetList);
            _signal.SignalToken(SitemapSetCacheKey);

            return count;
        }

        private async Task BuildSitemapRoutes(SitemapSetList sitemapSetList = null)
        {
            if (sitemapSetList == null)
            {
                sitemapSetList = await GetSitemapSetList();
            }
            _routes = new HashSet<string>();
            foreach (var sitemapSet in sitemapSetList.SitemapSets)
            {
                var rootPath = sitemapSet.RootPath.TrimStart('/');
                BuildNodeRoutes(sitemapSet.SitemapNodes, rootPath);
            }
        }

        private void BuildNodeRoutes(IList<SitemapNode> sitemapNodes, string rootPath)
        {
            foreach(var sitemapNode in sitemapNodes)
            {

                var path = String.Concat(rootPath, sitemapNode.Path);
                _routes.Add(path);
                if (sitemapNode.ChildNodes != null)
                {
                    BuildNodeRoutes(sitemapNode.ChildNodes, rootPath);
                }
            }
        }

        private async Task<SitemapSetList> GetSitemapSetList()
        {
            SitemapSetList treeList;

            if (!_memoryCache.TryGetValue(SitemapSetCacheKey, out treeList))
            {
                var session = GetSession();

                treeList = await session.Query<SitemapSetList>().FirstOrDefaultAsync();

                if (treeList == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(SitemapSetCacheKey, out treeList))
                        {
                            treeList = new SitemapSetList();
                            session.Save(treeList);
                            _memoryCache.Set(SitemapSetCacheKey, treeList);
                            _signal.SignalToken(SitemapSetCacheKey);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(SitemapSetCacheKey, treeList);
                    _signal.SignalToken(SitemapSetCacheKey);
                }
            }

            return treeList;
        }

        private YesSql.ISession GetSession()
        {
            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            return httpContextAccessor.HttpContext.RequestServices.GetService<YesSql.ISession>();
        }
    }
}
