using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Lucene;

public static class LuceneSearchPermissions
{
    public static readonly Permission ManageLuceneIndexes = new("ManageLuceneIndexes", LocalizationSource.Create("Manage Lucene Indexes", typeof(LuceneSearchPermissions)));

    public static readonly Permission QueryLuceneApi = new("QueryLuceneApi", LocalizationSource.Create("Query Lucene Api", typeof(LuceneSearchPermissions)), new[] { ManageLuceneIndexes });
}
