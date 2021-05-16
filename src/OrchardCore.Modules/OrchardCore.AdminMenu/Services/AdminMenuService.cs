using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.AdminMenu.Models;
using OrchardCore.Documents;

namespace OrchardCore.AdminMenu.Services
{
    public class AdminMenuService : IAdminMenuService
    {
        private readonly IDocumentManager<AdminMenuList> _documentManager;

        public AdminMenuService(IDocumentManager<AdminMenuList> documentManager) => _documentManager = documentManager;

        /// <summary>
        /// Loads the admin menus from the store for updating and that should not be cached.
        /// </summary>
        public Task<AdminMenuList> LoadAdminMenuListAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the admin menus from the cache for sharing and that should not be updated.
        /// </summary>
        public Task<AdminMenuList> GetAdminMenuListAsync() => _documentManager.GetOrCreateImmutableAsync();

        public async Task SaveAsync(Models.AdminMenu tree)
        {
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

            await _documentManager.UpdateAsync(adminMenuList);
        }

        public Models.AdminMenu GetAdminMenuById(AdminMenuList adminMenuList, string id)
        {
            return adminMenuList.AdminMenu.FirstOrDefault(x => String.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> DeleteAsync(Models.AdminMenu tree)
        {
            var adminMenuList = await LoadAdminMenuListAsync();

            var count = adminMenuList.AdminMenu.RemoveAll(x => String.Equals(x.Id, tree.Id, StringComparison.OrdinalIgnoreCase));

            await _documentManager.UpdateAsync(adminMenuList);

            return count;
        }
    }
}
