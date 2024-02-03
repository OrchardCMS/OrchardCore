using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Services
{
    public class ContentItemIdHandleProvider : IContentHandleProvider
    {
        public int Order => 0;

        public Task<string> GetContentItemIdAsync(string handle)
        {
            if (handle.StartsWith("contentitemid:", System.StringComparison.OrdinalIgnoreCase))
            {
                var contentItemId = handle[14..];

                return Task.FromResult(contentItemId);
            }

            return Task.FromResult<string>(null);
        }
    }
}
