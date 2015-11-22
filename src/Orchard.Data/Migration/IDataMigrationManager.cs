using Orchard.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Data.Migration
{
    public interface IDataMigrationManager : IDependency
    {
        /// <summary>
        /// Whether a feature has already been installed, i.e. one of its Data Migration class has already been processed
        /// </summary>
        bool IsFeatureAlreadyInstalled(string feature);

        /// <summary>
        /// Returns the features which have at least one Data Migration class with a corresponding Upgrade method to be called
        /// </summary>
        Task<IEnumerable<string>> GetFeaturesThatNeedUpdate();

        /// <summary>
        /// Updates the database to the latest version for the specified feature
        /// </summary>
        Task UpdateAsync(string feature);

        /// <summary>
        /// Updates the database to the latest version for the specified features
        /// </summary>
        Task UpdateAsync(IEnumerable<string> features);

        /// <summary>
        /// Execute a script to delete any information relative to the feature
        /// </summary>
        /// <param name="feature"></param>
        Task Uninstall(string feature);
    }
}