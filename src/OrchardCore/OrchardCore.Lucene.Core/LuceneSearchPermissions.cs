using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Lucene;

public static class LuceneSearchPermissions
{
    public static readonly Permission ManageLuceneIndexes = new("ManageLuceneIndexes", new LocalizationSource("Manage Lucene Indexes", typeof(LuceneSearchPermissions)));

    public static readonly Permission QueryLuceneApi = new("QueryLuceneApi", new LocalizationSource("Query Lucene Api", typeof(LuceneSearchPermissions)), new[] { ManageLuceneIndexes });
}
