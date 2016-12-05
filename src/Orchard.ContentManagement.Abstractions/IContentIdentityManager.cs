using System.Threading.Tasks;

namespace Orchard.ContentManagement
{
    public interface IContentIdentityManager
    {
        Task<ContentItem> LoadContentItemAsync(string key, string value);
    }
}
