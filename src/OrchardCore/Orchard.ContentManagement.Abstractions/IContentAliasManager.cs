using System.Threading.Tasks;

namespace Orchard.ContentManagement
{
    public interface IContentAliasManager
    {
        Task<string> GetContentItemIdAsync(string alias);
    }
}
