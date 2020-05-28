using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Sitemaps.Services
{
    /// <summary>
    /// Helper services to provides path validation, and sitemap slugs.
    /// </summary>
    public interface ISitemapHelperService
    {
        Task ValidatePathAsync(string path, IUpdateModel updater, string sitemapId = null);

        string GetSitemapSlug(string name);
    }
}
