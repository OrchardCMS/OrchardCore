using System.Threading.Tasks;

namespace Orchard.ContentManagement
{
    public interface IContentIdentityProvider
    {
        Task<ContentItem> LoadContentItemAsync(string key, string value);
    }
}
