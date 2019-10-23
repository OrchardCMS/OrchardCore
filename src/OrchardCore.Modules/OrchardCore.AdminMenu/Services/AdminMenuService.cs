using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.AdminMenu.Models;
using OrchardCore.Environment.Cache;
using YesSql;

namespace OrchardCore.AdminMenu
{
    public class AdminMenuService : IAdminMenuService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly ISession _session;
        private const string AdminMenuCacheKey = nameof(AdminMenuService);

        private AdminMenuList _adminMenuList;

        public AdminMenuService(
            ISignal signal,
            ISession session,
            IMemoryCache memoryCache
            )
        {
            _signal = signal;
            _session = session;
            _memoryCache = memoryCache;
        }

        public IChangeToken ChangeToken => _signal.GetToken(AdminMenuCacheKey);

        public async Task<List<Models.AdminMenu>> GetAsync()
        {
            return (await GetAdminMenuListAsync()).AdminMenu;
        }

        public async Task SaveAsync(Models.AdminMenu tree)
        {
            if (tree.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var adminMenuList = await LoadAdminMenuListAsync();

            var preexisting = adminMenuList.AdminMenu.FirstOrDefault(x => String.Equals(x.Id, tree.Id, StringComparison.OrdinalIgnoreCase));

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

            _session.Save(adminMenuList);

            _memoryCache.Set(AdminMenuCacheKey, adminMenuList);
            _signal.SignalToken(AdminMenuCacheKey);
        }

        public Models.AdminMenu GetAdminMenuById(AdminMenuList adminMenuList, string id)
        {
            return adminMenuList.AdminMenu
                .FirstOrDefault(x => String.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> DeleteAsync(Models.AdminMenu tree)
        {
            if (tree.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var adminMenuList = await LoadAdminMenuListAsync();

            var count = adminMenuList.AdminMenu.RemoveAll(x => String.Equals(x.Id, tree.Id, StringComparison.OrdinalIgnoreCase));

            _session.Save(adminMenuList);

            _memoryCache.Set(AdminMenuCacheKey, adminMenuList);
            _signal.SignalToken(AdminMenuCacheKey);

            return count;
        }

        /// <summary>
        /// Returns the document from the database to be updated
        /// </summary>
        public async Task<AdminMenuList> LoadAdminMenuListAsync()
        {
            return _adminMenuList = _adminMenuList ?? await _session.Query<AdminMenuList>().FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        public async Task<AdminMenuList> GetAdminMenuListAsync()
        {
            AdminMenuList adminMenuList;

            if (!_memoryCache.TryGetValue(AdminMenuCacheKey, out adminMenuList))
            {
                adminMenuList = await _session.Query<AdminMenuList>().FirstOrDefaultAsync();

                if (adminMenuList == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(AdminMenuCacheKey, out adminMenuList))
                        {
                            adminMenuList = new AdminMenuList();
                            _session.Save(adminMenuList);
                            _memoryCache.Set(AdminMenuCacheKey, adminMenuList);
                            _signal.SignalToken(AdminMenuCacheKey);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(AdminMenuCacheKey, adminMenuList);
                    _signal.SignalToken(AdminMenuCacheKey);
                }

                foreach (var adminMenu in adminMenuList.AdminMenu)
                {
                    adminMenu.IsReadonly = true;
                }
            }

            return adminMenuList;
        }
    }
}
