using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Data.Migration;

/// <summary>
/// Provides an extension methods for <see cref="IDataMigrationManager"/>.
/// </summary>
public static class DataMigrationManagerExtensions
{
    /// <summary>
    /// Updates the database to the latest version for the specified features.
    /// </summary>
    /// <param name="dataMigrationManager">The <see cref="IDataMigrationManager"/>.</param>
    /// <param name="features">The features to be updated.</param>
    public static async Task UpdateAsync(this IDataMigrationManager dataMigrationManager, IEnumerable<string> features)
    {
        ArgumentNullException.ThrowIfNull(dataMigrationManager, nameof(dataMigrationManager));
        ArgumentNullException.ThrowIfNull(features, nameof(features));

        await Task.WhenAll(features.Select(dataMigrationManager.UpdateAsync));
    }
}
