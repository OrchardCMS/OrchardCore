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
    public class AdminMenuService : IAdminMenuService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IServiceProvider _serviceProvider;
        private readonly IClock _clock;
        private const string AdminMenuCacheKey = "AdminMenuervice";

        public AdminMenuService(
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

        public async Task<List<Models.AdminMenu>> GetAsync()
        {
            return (await GetAdminMenuList()).AdminMenu;
        }

        public async Task SaveAsync(Models.AdminMenu tree)
        {
            var adminMenuList = await GetAdminMenuList();
            var session = GetSession();


            var preexisting = adminMenuList.AdminMenu.Where(x => x.Id == tree.Id).FirstOrDefault();

            // it's new? add it
            if (preexisting == null)
            {
                adminMenuList.AdminMenu.Add(tree);
            }
            else // not new: replace it
            {
                var index = adminMenuList.AdminMenu.IndexOf(preexisting);
                adminMenuList.AdminMenu[index] = tree;
            }

            session.Save(adminMenuList);

            _memoryCache.Set(AdminMenuCacheKey, adminMenuList);
            _signal.SignalToken(AdminMenuCacheKey);
        }

        public async Task<Models.AdminMenu> GetByIdAsync(string id)
        {
            return (await GetAdminMenuList())
                .AdminMenu
                .Where(x => String.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }


        public async Task<int> DeleteAsync(Models.AdminMenu tree)
        {
            var adminMenuList = await GetAdminMenuList();
            var session = GetSession();

            var count = adminMenuList.AdminMenu.RemoveAll(x => String.Equals(x.Id, tree.Id));

            session.Save(adminMenuList);

            _memoryCache.Set(AdminMenuCacheKey, adminMenuList);
            _signal.SignalToken(AdminMenuCacheKey);

            return count;
        }

        private async Task<AdminMenuList> GetAdminMenuList()
        {
            AdminMenuList treeList;

            if (!_memoryCache.TryGetValue(AdminMenuCacheKey, out treeList))
            {
                var session = GetSession();

                treeList = await session.Query<AdminMenuList>().FirstOrDefaultAsync();

                if (treeList == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(AdminMenuCacheKey, out treeList))
                        {
                            treeList = new AdminMenuList();
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
