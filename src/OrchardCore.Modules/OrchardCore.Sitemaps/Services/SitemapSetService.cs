using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;
using OrchardCore.Sitemaps.Models;
using YesSql;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapSetService : ISitemapSetService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IServiceProvider _serviceProvider;
        private readonly SitemapEntries _sitemapEntries;
        private const string SitemapSetCacheKey = "SitemapSetService";

        public SitemapSetService(
            ISignal signal,
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache,
            SitemapEntries sitemapEntries
            )
        {
            _signal = signal;
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
            _sitemapEntries = sitemapEntries;
        }

        public IChangeToken ChangeToken => _signal.GetToken(SitemapSetCacheKey);

        public async Task<IList<SitemapSet>> GetAsync()
        {
            return (await GetSitemapSetList()).SitemapSets;
        }

        public async Task SaveAsync(SitemapSet tree)
        {
            var sitemapSetList = await GetSitemapSetList();
            var session = GetSession();

            var preExisting = sitemapSetList.SitemapSets.FirstOrDefault(x => x.Id == tree.Id);

            // it's new? add it
            if (preExisting == null)
            {
                sitemapSetList.SitemapSets.Add(tree);
                BuildSitemapRouteEntries(tree.SitemapNodes, tree.RootPath.TrimStart('/'));
            }
            else // not new: replace it
            {
                DeleteSitemapRouteEntries(preExisting.SitemapNodes, preExisting.RootPath.TrimStart('/'));
                var index = sitemapSetList.SitemapSets.IndexOf(preExisting);
                sitemapSetList.SitemapSets[index] = tree;
                BuildSitemapRouteEntries(tree.SitemapNodes, tree.RootPath.TrimStart('/'));
            }

            session.Save(sitemapSetList);
            //TODO expire after a period of time. 2 hours seems reasonable.
            _memoryCache.Set(SitemapSetCacheKey, sitemapSetList);
            _signal.SignalToken(SitemapSetCacheKey);
        }

        public async Task<SitemapSet> GetByIdAsync(string id)
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

        public async Task<int> DeleteAsync(SitemapSet tree)
        {
            var sitemapSetList = await GetSitemapSetList();
            var session = GetSession();

            var sitemapSet = sitemapSetList.SitemapSets.FirstOrDefault(x => x.Id == tree.Id);
            DeleteSitemapRouteEntries(sitemapSet.SitemapNodes, sitemapSet.RootPath.TrimStart('/'));

            var count = sitemapSetList.SitemapSets.RemoveAll(x => x.Id == tree.Id);

            session.Save(sitemapSetList);
            _memoryCache.Set(SitemapSetCacheKey, sitemapSetList);
            _signal.SignalToken(SitemapSetCacheKey);

            return count;
        }

        public void BuildSitemapRouteEntries(IList<SitemapNode> sitemapNodes, string rootPath)
        {
            var entries = GetSitemapRouteEntries(sitemapNodes, rootPath);
            _sitemapEntries.AddEntries(entries);
        }

        private List<SitemapEntry> GetSitemapRouteEntries(IList<SitemapNode> sitemapNodes, string rootPath, List<SitemapEntry> sitemapEntries = null)
        {
            if (sitemapEntries == null)
            {
                sitemapEntries = new List<SitemapEntry>();
            }
            foreach (var sitemapNode in sitemapNodes)
            {
                var path = String.Concat(rootPath, sitemapNode.Path);
                sitemapEntries.Add(new SitemapEntry { Path = path, SitemapNodeId = sitemapNode.Id });
                if (sitemapNode.ChildNodes != null)
                {
                    GetSitemapRouteEntries(sitemapNode.ChildNodes, rootPath, sitemapEntries);
                }
            }
            return sitemapEntries;
        }

        private void DeleteSitemapRouteEntries(IList<SitemapNode> sitemapNodes, string rootPath)
        {
            var entries = GetSitemapRouteEntries(sitemapNodes, rootPath);
            _sitemapEntries.RemoveEntries(entries);
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

        //TODO why this is scoped? not singleton.
        private YesSql.ISession GetSession()
        {
            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            return httpContextAccessor.HttpContext.RequestServices.GetService<YesSql.ISession>();
        }
    }
}
