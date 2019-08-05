using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.AdminMenu.Models;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.AdminMenu
{
    public class AdminMenuService : IAdminMenuService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private const string AdminMenuCacheKey = "AdminMenuervice";

        public AdminMenuService(
            ISignal signal,
            IMemoryCache memoryCache)
        {
            _signal = signal;
            _memoryCache = memoryCache;
        }

        public IChangeToken ChangeToken => _signal.GetToken(AdminMenuCacheKey);

        public async Task<ImmutableArray<Models.AdminMenu>> GetAsync()
        {
            return (await GetAdminMenuListAsync()).AdminMenu;
        }

        public async Task SaveAsync(Models.AdminMenu tree)
        {
            var adminMenuList = await GetAdminMenuListAsync();

            var preexisting = adminMenuList.AdminMenu.Where(m => m.Id == tree.Id).FirstOrDefault();

            // it's new? add it
            if (preexisting == null)
            {
                adminMenuList.AdminMenu = adminMenuList.AdminMenu.Add(tree);
            }
            else // not new: replace it
            {
                adminMenuList.AdminMenu = adminMenuList.AdminMenu.Replace(preexisting, tree);
            }

            await SaveAsync(adminMenuList);
        }

        public async Task<Models.AdminMenu> GetByIdAsync(string id)
        {
            return (await GetAdminMenuListAsync())
                .AdminMenu
                .Where(m => String.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }

        public async Task<int> DeleteAsync(Models.AdminMenu tree)
        {
            var adminMenuList = await GetAdminMenuListAsync();

            var length = adminMenuList.AdminMenu.Length;
            adminMenuList.AdminMenu = adminMenuList.AdminMenu.RemoveAll(m => String.Equals(m.Id, tree.Id));

            await SaveAsync(adminMenuList);

            return length - adminMenuList.AdminMenu.Length;
        }

        private async Task<AdminMenuList> GetAdminMenuListAsync()
        {
            AdminMenuList treeList;

            if (!_memoryCache.TryGetValue(AdminMenuCacheKey, out treeList))
            {
                var changeToken = ChangeToken;
                treeList = await Session.Query<AdminMenuList>().FirstOrDefaultAsync();

                if (treeList == null)
                {
                    treeList = new AdminMenuList();
                    await SaveAsync(treeList);
                }
                else
                {
                    _memoryCache.Set(AdminMenuCacheKey, treeList, changeToken);
                }
            }

            return treeList;
        }

        private async Task SaveAsync(AdminMenuList treeList)
        {
            var session = Session;

            session.Save(treeList);
            await session.CommitAsync();
            _signal.SignalToken(AdminMenuCacheKey);
        }

        private ISession Session => ShellScope.Services.GetService<ISession>();
    }
}
