using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.AdminMenu
{
    /// <summary>
    /// Provides services to manage the admin menus.
    /// </summary>
    public interface IAdminMenuService
    {
        /// <summary>
        /// Returns all the admin menus for update.
        /// </summary>
        Task<Models.AdminMenuList> LoadAdminMenuListAsync();

        /// <summary>
        /// Returns all the admin menus in read-only.
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

        /// <summary>
        /// Gets a change token that is set when the admin menu has changed.
        /// </summary>
        IChangeToken ChangeToken { get; }
    }
}
