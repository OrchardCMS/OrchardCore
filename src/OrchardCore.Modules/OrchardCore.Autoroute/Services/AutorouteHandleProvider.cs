using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteHandleProvider : IContentHandleProvider
    {
        private readonly IAutorouteEntries _autorouteEntries;

        public AutorouteHandleProvider(IAutorouteEntries autorouteEntries)
        {
            _autorouteEntries = autorouteEntries;
        }

        public int Order => 10;

        public Task<string> GetContentItemIdAsync(string handle)
        {
            if (handle.StartsWith("slug:", System.StringComparison.OrdinalIgnoreCase))
            {
                handle = handle.Substring(5);

                if (!handle.StartsWith('/'))
                {
                    handle = "/" + handle;
                }

                if (_autorouteEntries.TryGetEntryByPath(handle, out var entry))
                {
                    // TODO this requires more work, and interface changes to support contained content items.
                    // as it will require returning the id and jsonPath.
                    return Task.FromResult(entry.ContentItemId);
                }
            }

            return Task.FromResult<string>(null);
        }
    }
}
