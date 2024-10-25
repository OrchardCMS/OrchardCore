using OrchardCore.Alias.Indexes;
using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Alias.Services;

public class AliasPartContentHandleProvider : IContentHandleProvider
{
    private readonly ISession _session;

    public AliasPartContentHandleProvider(ISession session)
    {
        _session = session;
    }

    public int Order => 100;

    public async Task<string> GetContentItemIdAsync(string handle)
    {
        if (handle.StartsWith("alias:", StringComparison.OrdinalIgnoreCase))
        {
            handle = handle[6..];

            var aliasPartIndex = await AliasPartContentHandleHelper.QueryAliasIndex(_session, handle);
            return aliasPartIndex?.ContentItemId;
        }

        return null;
    }
}

internal sealed class AliasPartContentHandleHelper
{
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
    internal static Task<ContentItem> QueryAliasIndex(ISession session, string alias) =>
        session.Query<ContentItem, AliasPartIndex>(x => x.Alias == alias.ToLowerInvariant()).FirstOrDefaultAsync();
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
}
