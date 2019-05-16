using System.Linq;

namespace OrchardCore.Settings
{
    public static class SiteExtensions
    {
        public static string[] GetManageableCultures(this ISite site)
        {
            return new[] { site.Culture }.Concat(site.SupportedCultures).Distinct().ToArray();
        }
    }
}
