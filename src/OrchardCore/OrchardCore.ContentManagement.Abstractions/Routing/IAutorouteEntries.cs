using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Routing
{
    public interface IAutorouteEntries
    {
        Task<string> TryGetContentItemIdAsync(string path);
        Task<string> TryGetPathAsync(string contentItemId);
        Task AddEntriesAsync(IEnumerable<AutorouteEntry> entries);
        Task RemoveEntriesAsync(IEnumerable<AutorouteEntry> entries);
    }
}
