using System.Threading.Tasks;

namespace OrchardCore.ContentManagement
{
    public interface IContentAliasManager
    {
        Task<string> GetContentItemIdAsync(string alias);
    }
}
