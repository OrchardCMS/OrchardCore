using System.Threading.Tasks;

namespace OrchardCore.ContentManagement
{
    public interface IContentHandleProvider
    {
        int Order { get; }
        Task<string> GetContentItemIdAsync(string handle);
    }
}
