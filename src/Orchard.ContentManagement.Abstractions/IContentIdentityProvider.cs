using System.Threading.Tasks;

namespace Orchard.ContentManagement
{
    public interface IContentIdentityProvider
    {
        Task<ContentItem> GetAsync(string key, string value);
    }
}
