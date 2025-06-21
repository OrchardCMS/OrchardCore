using OrchardCore.Entities;

namespace OrchardCore.Settings;

public static class SiteServiceExtensions
{
    /// <summary>
    /// Gets an instance of the specified settings if it exists.
    /// </summary>
    /// <typeparam name="T">The type of the settings to attempt to get.</typeparam>
    /// <param name="siteService">The site service.</param>
    /// <returns>An instance of the given type if one exists.</returns>
    public static async Task<T> GetSettingsAsync<T>(this ISiteService siteService) where T : new()
        => (await siteService.GetSiteSettingsAsync().ConfigureAwait(false)).As<T>();

    /// <summary>
    /// Gets an instance of the specified settings if it exists.
    /// </summary>
    /// <typeparam name="T">The type of the settings to attempt to get.</typeparam>
    /// <param name="siteService">The site service.</param>
    /// <param name="name">The entry name.</param>
    /// <returns>An instance of the given type if one exists.</returns>
    public static async Task<T> GetSettingsAsync<T>(this ISiteService siteService, string name) where T : new()
        => (await siteService.GetSiteSettingsAsync().ConfigureAwait(false)).As<T>(name);

    /// <summary>
    /// Synchronously gets the site settings for reading.
    /// </summary>
    /// <remarks>
    /// This method should only be used in synchronous code paths. Prefer using <see cref="ISiteService.GetSiteSettingsAsync"/> in asynchronous code.
    /// </remarks>
    public static ISite GetSiteSettings(this ISiteService siteService)
    {
        // Site settings are preloaded by the tenant event handler PreloadSiteSettingsTenantEventHandler to ensure
        // that the database access is asynchronous. This ensures that site settings can be safely retrieved
        // synchronously afterward.
        var task = siteService.GetSiteSettingsAsync();

        return task.IsCompletedSuccessfully ? task.Result : task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronously gets an instance of the specified settings for reading, if it exists.
    /// </summary>
    /// <typeparam name="T">The type of the settings to attempt to get.</typeparam>
    /// <param name="siteService">The site service.</param>
    /// <returns>An instance of the given type if one exists.</returns>
    /// <remarks>
    /// This method should only be used in synchronous code paths. Prefer using <see cref="GetSettingsAsync{T}(ISiteService)"/> in asynchronous code.
    /// </remarks>
    public static T GetSettings<T>(this ISiteService siteService) where T : new()
        => siteService.GetSiteSettings().As<T>();
}
