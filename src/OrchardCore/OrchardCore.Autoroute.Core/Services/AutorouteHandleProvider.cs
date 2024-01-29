using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Autoroute.Core.Services
{
    public class AutorouteHandleProvider : IContentHandleProvider
    {
        private readonly IAutorouteEntries _autorouteEntries;

        public AutorouteHandleProvider(IAutorouteEntries autorouteEntries) => _autorouteEntries = autorouteEntries;

        public int Order => 10;

        public async Task<string> GetContentItemIdAsync(string handle)
        {
            if (handle.StartsWith("slug:", System.StringComparison.OrdinalIgnoreCase))
            {
                handle = handle[5..];

                if (!handle.StartsWith('/'))
                {
                    handle = "/" + handle;
                }

                (var found, var entry) = await _autorouteEntries.TryGetEntryByPathAsync(handle);

                if (found)
                {
                    // TODO this requires more work, and interface changes to support contained content items.
                    // as it will require returning the id and jsonPath.
                    return entry.ContentItemId;
                }
            }

            return null;
        }
    }
}
