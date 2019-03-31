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
using OrchardCore.Sitemaps.Routing;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapSetService : ISitemapSetService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISitemapRoute _sitemapRoute;
        private const string SitemapSetCacheKey = "SitemapSetService";

        public SitemapSetService(
            ISignal signal,
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache,
            ISitemapRoute sitemapRoute)
        {
            _signal = signal;
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
            _sitemapRoute = sitemapRoute;
        }

        public IChangeToken ChangeToken => _signal.GetToken(SitemapSetCacheKey);


        public async Task<IList<Models.SitemapSet>> GetAsync()
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
            await _sitemapRoute.BuildSitemapRoutes(sitemapSetList.SitemapSets);
            _memoryCache.Set(SitemapSetCacheKey, sitemapSetList);
            _signal.SignalToken(SitemapSetCacheKey);
        }

        public async Task<Models.SitemapSet> GetByIdAsync(string id)
        {
            return (await GetSitemapSetList())
                .SitemapSets
                .Where(x => x.Id == id)
                .FirstOrDefault();
        }

        public async Task<SitemapNode> GetSitemapNodeByIdAsync(string nodeId)
        {
            var trees = (await GetSitemapSetList()).SitemapSets;
            foreach (var tree in trees)
            {
                var node = tree.GetSitemapNodeById(nodeId);
                if (node != null)
                    return node;
            }
            return null;
        }

        public async Task<int> DeleteAsync(Models.SitemapSet tree)
        {
            var sitemapSetList = await GetSitemapSetList();
            var session = GetSession();

            var count = sitemapSetList.SitemapSets.RemoveAll(x => x.Id == tree.Id);

            session.Save(sitemapSetList);
            await _sitemapRoute.BuildSitemapRoutes(sitemapSetList.SitemapSets);
            _memoryCache.Set(SitemapSetCacheKey, sitemapSetList);
            _signal.SignalToken(SitemapSetCacheKey);

            return count;
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
                    treeList.SetNodeSet();
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
