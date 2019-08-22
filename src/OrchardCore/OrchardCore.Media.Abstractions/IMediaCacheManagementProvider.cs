using System.Threading.Tasks;

namespace OrchardCore.Media
{
    public interface IMediaCacheManagementProvider
    {
        string Name { get; }
        dynamic GetDisplayModel();
        Task<bool> ClearCacheAsync();
    }
}
