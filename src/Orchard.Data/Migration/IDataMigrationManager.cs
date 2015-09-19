using Orchard.DependencyInjection;
using System.Collections.Generic;

namespace Orchard.Data.Migration {
    public interface IDataMigrationManager : IDependency {
        /// <summary>
        /// Whether a feature has already been installed, i.e. one of its Data Migration class has already been processed
        /// </summary>
        bool IsFeatureAlreadyInstalled(string feature);

        /// <summary>
        /// Returns the features which have at least one Data Migration class with a corresponding Upgrade method to be called
        /// </summary>
        IEnumerable<string> GetFeaturesThatNeedUpdate();

        /// <summary>
        /// Updates the database to the latest version for the specified feature
        /// </summary>
        void Update(string feature);

        /// <summary>
        /// Updates the database to the latest version for the specified features
        /// </summary>
        void Update(IEnumerable<string> features);

        /// <summary>
        /// Execute a script to delete any information relative to the feature
        /// </summary>
        /// <param name="feature"></param>
        void Uninstall(string feature);
    }
}