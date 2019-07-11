using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings
{
    public static class CustomSettingsExtensions
    {
        public static async Task<T> GetCustomSettingsAsync<T>(this ISiteService siteService, string name = null)
            where T : ContentPart
        {
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }

            var siteSettings = await siteService.GetSiteSettingsAsync();

            return siteSettings
                .As<ContentItem>(name)
                .As<T>();
        }
    }
}
