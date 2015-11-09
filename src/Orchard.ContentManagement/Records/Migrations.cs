using Orchard.Data.Migration;

namespace Orchard.ContentManagement.Records
{
    public class Migrations : DataMigrations
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(ContentItemVersionRecordIndex), table => table
                .Column<int>("ContentItemRecordId")
                .Column<int>("Latest")
                .Column<int>("Number")
                .Column<int>("Published")
                .Column<string>("ContentType")
            );

            return 1;
        }
    }
}
