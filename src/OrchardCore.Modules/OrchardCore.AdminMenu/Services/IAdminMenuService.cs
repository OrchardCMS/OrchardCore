using System.Threading.Tasks;

namespace OrchardCore.AdminMenu.Services
{
    /// <summary>
    /// Provides services to manage the admin menus.
    /// </summary>
    public interface IAdminMenuService
    {
        /// <summary>
        /// Loads the admin menus from the store for updating and that should not be cached.
        /// </summary>
        Task<Models.AdminMenuList> LoadAdminMenuListAsync();

        /// <summary>
        /// Gets the admin menus from the cache for sharing and that should not be updated.
        /// </summary>
        Task<Models.AdminMenuList> GetAdminMenuListAsync();

        /// <summary>
        /// Persist an admin menu
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        Task SaveAsync(Models.AdminMenu tree);

        /// <summary>
        /// Returns an admin menu.
        /// </summary>
        Models.AdminMenu GetAdminMenuById(Models.AdminMenuList adminMenuList, string id);

        /// <summary>
        /// Deletes an admin menu
        /// </summary>
        /// <param name="tree"></param>
        /// <returns>The count of deleted items</returns>
        Task<int> DeleteAsync(Models.AdminMenu tree);
    }
}
