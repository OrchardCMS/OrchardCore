using System.Threading.Tasks;

namespace Orchard.ContentManagement
{
    public interface IContentAliasProvider
    {
        int Order { get; }
        Task<string> GetContentItemIdAsync(string alias);
    }
}
