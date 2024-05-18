using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Alias.Indexes;
using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Alias.Services
{
    internal sealed class AliasPartContentHandleHelper
    {
        private static readonly SemaphoreSlim _yesSqlLock = new(1, 1);

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        internal static async Task<ContentItem> QueryAliasIndexAsync(ISession session, string alias)
        {
            // NOTE: YesSql/Dapper does not support parallel or concurrent requests.
            // Doing so can cause an `InvalidOperationException`, with the connection stuck within a "connecting" state.
            await _yesSqlLock.WaitAsync();
            try
            {
                return await session.Query<ContentItem, AliasPartIndex>(x => x.Alias == alias.ToLowerInvariant()).FirstOrDefaultAsync();
            }
            finally
            {
                _yesSqlLock?.Release();
            }
        }
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
    }
}
