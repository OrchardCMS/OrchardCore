using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.AdminTrees.Models;

namespace OrchardCore.AdminTrees
{
    /// <summary>
    /// Provides services to manage the admin trees.
    /// </summary>
    public interface IAdminTreeService
    {

        /// <summary>
        /// Returns all the admin trees
        /// </summary>
        Task<List<AdminTree>> GetAsync();

        /// <summary>
        /// Persist an admin tree
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        Task SaveAsync(AdminTree tree);

        /// <summary>
        /// Returns an admin tree.
        /// </summary>
        Task<AdminTree> GetByIdAsync(string id);

        /// <summary>
        /// Deletes an admin tree
        /// </summary>
        /// <param name="tree"></param>
        /// <returns>The count of deleted items</returns>
        Task<int> DeleteAsync(AdminTree tree);

        /// <summary>
        /// Gets a change token that is set when the admin tree has changed.
        /// </summary>
        IChangeToken ChangeToken { get; }

        
    }
}
