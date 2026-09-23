using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Lucene;

public static class LuceneSearchPermissions
{
    public static readonly Permission ManageLuceneIndexes = new("ManageLuceneIndexes", LocalizedString.Create("Manage Lucene Indexes", typeof(LuceneSearchPermissions)));

    public static readonly Permission QueryLuceneApi = new("QueryLuceneApi", LocalizedString.Create("Query Lucene Api", typeof(LuceneSearchPermissions)), new[] { ManageLuceneIndexes });
}
