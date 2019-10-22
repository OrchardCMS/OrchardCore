using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapContentItemValidationProvider
    {
        Task<bool> ValidateContentItem(ContentItem contentItem);
    }
}
