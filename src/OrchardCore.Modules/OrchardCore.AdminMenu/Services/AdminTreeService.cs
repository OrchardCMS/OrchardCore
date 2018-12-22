using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.AdminMenu.Models;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.AdminMenu
{
    public class AdminMenuervice : IAdminMenuervice
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IServiceProvider _serviceProvider;
        private readonly IClock _clock;
        private const string AdminMenuCacheKey = "AdminMenuervice";

        public AdminMenuervice(
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


        public IChangeToken ChangeToken => _signal.GetToken(AdminMenuCacheKey);

        public async Task<List<AdminTree>> GetAsync()
        {
            return (await GetAdminTreeList()).AdminMenu;
        }

        public async Task SaveAsync(AdminTree tree)
        {
            var adminTreeList = await GetAdminTreeList();
            var session = GetSession();

            
            var preexisting = adminTreeList.AdminMenu.Where(x => x.Id == tree.Id).FirstOrDefault();

            // it's new? add it
            if (preexisting == null) 
            {
                adminTreeList.AdminMenu.Add(tree);
            }
            else // not new: replace it
            {
                var index = adminTreeList.AdminMenu.IndexOf(preexisting);
                adminTreeList.AdminMenu[index] = tree;
            }

            session.Save(adminTreeList);

            _memoryCache.Set(AdminMenuCacheKey, adminTreeList);
            _signal.SignalToken(AdminMenuCacheKey);
        }


        public async Task<AdminTree> GetByIdAsync(string id)
        {
            return (await GetAdminTreeList())
                .AdminMenu
                .Where(x => String.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }

        
        public async Task<int> DeleteAsync(AdminTree tree)
        {
            var adminTreeList = await GetAdminTreeList();
            var session = GetSession();

            var count = adminTreeList.AdminMenu.RemoveAll(x => String.Equals(x.Id, tree.Id));

            session.Save(adminTreeList);

            _memoryCache.Set(AdminMenuCacheKey, adminTreeList);
            _signal.SignalToken(AdminMenuCacheKey);

            return count;
        }



        private async Task<AdminTreeList> GetAdminTreeList()
        {
            AdminTreeList treeList;

            if (!_memoryCache.TryGetValue(AdminMenuCacheKey, out treeList))
            {
                var session = GetSession();

                treeList = await session.Query<AdminTreeList>().FirstOrDefaultAsync();

                if (treeList == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(AdminMenuCacheKey, out treeList))
                        {
                            treeList = new AdminTreeList();
                            session.Save(treeList);
                            _memoryCache.Set(AdminMenuCacheKey, treeList);
                            _signal.SignalToken(AdminMenuCacheKey);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(AdminMenuCacheKey, treeList);
                    _signal.SignalToken(AdminMenuCacheKey);
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
