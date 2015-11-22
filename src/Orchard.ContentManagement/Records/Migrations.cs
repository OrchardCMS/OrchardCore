using Orchard.Data.Migration;

namespace Orchard.ContentManagement.Records
{
    public class Migrations : DataMigrations
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(ContentItemIndex), table => table
                .Column<int>("ContentItemId")
                .Column<int>("Latest")
                .Column<int>("Number")
                .Column<int>("Published")
                .Column<string>("ContentType", column => column.WithLength(255))
            );

            return 1;
        }
    }
}