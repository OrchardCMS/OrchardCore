using System.Collections.Generic;

namespace OrchardCore.Tenant
{
    public interface ITenantSettingsManager
    {
        /// <summary>
        /// Retrieves all tenant settings stored.
        /// </summary>
        /// <returns>All tenant settings.</returns>
        IEnumerable<TenantSettings> LoadSettings();

        /// <summary>
        /// Persists tenant settings to the storage.
        /// </summary>
        /// <param name="settings">The tenant settings to store.</param>
        void SaveSettings(TenantSettings settings);
    }
}