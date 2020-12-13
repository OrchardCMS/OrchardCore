using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Routing
{
    public interface IAutorouteEntries
    {
        Task<(bool, AutorouteEntry)> TryGetEntryByPathAsync(string path);
        Task<(bool, AutorouteEntry)> TryGetEntryByContentItemIdAsync(string contentItemId);
        Task UpdateEntriesAsync();
    }
}
