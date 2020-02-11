using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Services
{
    public class ContentItemIdAliasProvider : IContentAliasProvider
    {
        public int Order => 0;

        public Task<string> GetContentItemIdAsync(string alias)
        {
            if (alias.StartsWith("contentitemid:", System.StringComparison.OrdinalIgnoreCase))
            {
                string contentItemId = alias.Substring(14);

                return Task.FromResult(contentItemId);
            }

            return Task.FromResult<string>(null);
        }
    }
}
