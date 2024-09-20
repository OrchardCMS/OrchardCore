using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Lucene;

public static class LuceneSearchPermissions
{
    public static readonly Permission ManageLuceneIndexes = new("ManageLuceneIndexes", "Manage Lucene Indexes");

    public static readonly Permission QueryLuceneApi = new("QueryLuceneApi", "Query Lucene Api", new[] { ManageLuceneIndexes });
}
