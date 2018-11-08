using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.AdminTrees.Models;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.AdminTrees
{
    public class AdminTreeService : IAdminTreeService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IServiceProvider _serviceProvider;
        private readonly IClock _clock;
        private const string AdminTreesCacheKey = "AdminTreeService";

        public AdminTreeService(
            ISignal signal,
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache,
            IClock clock)
        {
            _signal = signal;
            _serviceProvider = serviceProvider;
            _clock = clock;
            _memoryCache = memoryCache;
        }

        public IChangeToken ChangeToken => _signal.GetToken(AdminTreesCacheKey);

        public async Task<List<AdminTree>> GetAsync()
        {
            return (await GetAdminTreeList()).AdminTrees;
        }

        public async Task SaveAsync(AdminTree tree)
        {
            var adminTreeList = await GetAdminTreeList();
            var session = GetSession();

            var preexisting = adminTreeList.AdminTrees.Where(x => x.Id == tree.Id).FirstOrDefault();

            // it's new? add it
            if (preexisting == null)
            {
                adminTreeList.AdminTrees.Add(tree);
            }
            else // not new: replace it
            {
                var index = adminTreeList.AdminTrees.IndexOf(preexisting);
                adminTreeList.AdminTrees[index] = tree;
            }

            session.Save(adminTreeList);

            await _signal.SignalTokenAsync(AdminTreesCacheKey);
            _memoryCache.Set(AdminTreesCacheKey, adminTreeList, ChangeToken);
        }

        public async Task<AdminTree> GetByIdAsync(string id)
        {
            return (await GetAdminTreeList())
                .AdminTrees
                .Where(x => String.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }

        public async Task<int> DeleteAsync(AdminTree tree)
        {
            var adminTreeList = await GetAdminTreeList();
            var session = GetSession();

            var count = adminTreeList.AdminTrees.RemoveAll(x => String.Equals(x.Id, tree.Id));

            session.Save(adminTreeList);

            await _signal.SignalTokenAsync(AdminTreesCacheKey);
            _memoryCache.Set(AdminTreesCacheKey, adminTreeList, ChangeToken);

            return count;
        }

        private async Task<AdminTreeList> GetAdminTreeList()
        {
            AdminTreeList treeList;

            if (!_memoryCache.TryGetValue(AdminTreesCacheKey, out treeList))
            {
                var session = GetSession();

                treeList = await session.Query<AdminTreeList>().FirstOrDefaultAsync();

                if (treeList == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(AdminTreesCacheKey, out treeList))
                        {
                            treeList = new AdminTreeList();
                            session.Save(treeList);
                            _memoryCache.Set(AdminTreesCacheKey, treeList, ChangeToken);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(AdminTreesCacheKey, treeList, ChangeToken);
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
