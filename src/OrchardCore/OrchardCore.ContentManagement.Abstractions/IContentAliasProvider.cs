using System.Threading.Tasks;

namespace OrchardCore.ContentManagement
{
    public interface IContentAliasProvider
    {
        int Order { get; }
        Task<string> GetContentItemIdAsync(string alias);
    }
}
