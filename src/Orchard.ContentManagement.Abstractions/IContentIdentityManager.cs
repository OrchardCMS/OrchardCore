using System.Threading.Tasks;

namespace Orchard.ContentManagement
{
    public interface IContentIdentityManager
    {
        Task<ContentItem> GetAsync(ContentIdentity identity);
    }
}
