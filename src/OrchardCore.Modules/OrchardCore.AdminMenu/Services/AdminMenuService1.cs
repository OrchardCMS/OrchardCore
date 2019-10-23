//using System;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Primitives;
//using OrchardCore.AdminMenu.Models;
//using OrchardCore.Environment.Cache;
//using OrchardCore.Environment.Shell.Scope;
//using YesSql;

//namespace OrchardCore.AdminMenu
//{
//    public class AdminMenuService1 : IAdminMenuService
//    {
//        private readonly IMemoryCache _memoryCache;
//        private readonly ISignal _signal;
//        private const string AdminMenuCacheKey = "AdminMenuervice";

//        public AdminMenuService1(
//            ISignal signal,
//            IMemoryCache memoryCache)
//        {
//            _signal = signal;
//            _memoryCache = memoryCache;
//        }

//        public IChangeToken ChangeToken => _signal.GetToken(AdminMenuCacheKey);

//        public async Task<ImmutableArray<Models.AdminMenu>> GetAsync()
//        {
//            return (await GetAdminMenuListAsync()).AdminMenu;
//        }

//        public async Task SaveAsync(Models.AdminMenu tree)
//        {
//            var adminMenuList = await GetAdminMenuListAsync();

//            var preexisting = adminMenuList.AdminMenu.Where(m => m.Id == tree.Id).FirstOrDefault();

//            // it's new? add it
//            if (preexisting == null)
//            {
//                adminMenuList.AdminMenu = adminMenuList.AdminMenu.Add(tree);
//            }
//            else // not new: replace it
//            {
//                adminMenuList.AdminMenu = adminMenuList.AdminMenu.Replace(preexisting, tree);
//            }

//            Session.Save(adminMenuList);
//            _signal.DeferredSignalToken(AdminMenuCacheKey);
//        }

//        public async Task<Models.AdminMenu> GetByIdAsync(string id)
//        {
//            return (await GetAdminMenuListAsync())
//                .AdminMenu
//                .Where(m => String.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase))
//                .FirstOrDefault()
//                ?.Clone();
//        }

//        public async Task<int> DeleteAsync(Models.AdminMenu tree)
//        {
//            var adminMenuList = await GetAdminMenuListAsync();

//            var length = adminMenuList.AdminMenu.Length;
//            adminMenuList.AdminMenu = adminMenuList.AdminMenu.RemoveAll(m => String.Equals(m.Id, tree.Id));
//            var removed = length - adminMenuList.AdminMenu.Length;

//            Session.Save(adminMenuList);
//            _signal.DeferredSignalToken(AdminMenuCacheKey);

//            return removed;
//        }

//        private async Task<AdminMenuList> GetAdminMenuListAsync()
//        {
//            AdminMenuList treeList;

//            if (!_memoryCache.TryGetValue(AdminMenuCacheKey, out treeList))
//            {
//                var session = Session;

//                var changeToken = ChangeToken;
//                treeList = await session.Query<AdminMenuList>().FirstOrDefaultAsync();

//                if (treeList == null)
//                {
//                    treeList = new AdminMenuList();

//                    session.Save(treeList);
//                    _signal.DeferredSignalToken(AdminMenuCacheKey);
//                }
//                else
//                {
//                    _memoryCache.Set(AdminMenuCacheKey, treeList, changeToken);
//                }
//            }

//            return treeList;
//        }

//        private ISession Session => ShellScope.Services.GetService<ISession>();
//    }
//}
