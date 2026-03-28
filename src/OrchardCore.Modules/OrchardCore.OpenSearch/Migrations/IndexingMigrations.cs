using OrchardCore.Data.Migration;

namespace OrchardCore.OpenSearch.Migrations;

internal sealed class IndexingMigrations : DataMigration
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
    public int Create()
    {
        return 1;
    }
}
