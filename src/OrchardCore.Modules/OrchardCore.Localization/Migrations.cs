using OrchardCore.Data.Migration;
using OrchardCore.Localization.Indexes;

namespace OrchardCore.Localization
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(CultureIndex), table => table
                .Column<string>("Culture")
            );

            return 1;
        }

    }
}