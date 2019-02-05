using System;
using OrchardCore.Data.Migration;
using OrchardCore.ContentFields.Indexing;

namespace OrchardCore.ContentFields
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(TextFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(255))
                .Column<string>("ContentPart", column => column.WithLength(255))
                .Column<string>("ContentField", column => column.WithLength(255))
                .Column<string>("Text", column => column.Nullable().WithLength(4000))
                .Column<string>("RichText", column => column.Nullable().Unlimited())
            );

            SchemaBuilder.CreateMapIndexTable(nameof(BooleanFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(255))
                .Column<string>("ContentPart", column => column.WithLength(255))
                .Column<string>("ContentField", column => column.WithLength(255))
                .Column<bool>("Boolean", column => column.Nullable())
            );

            SchemaBuilder.CreateMapIndexTable(nameof(NumericFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(255))
                .Column<string>("ContentPart", column => column.WithLength(255))
                .Column<string>("ContentField", column => column.WithLength(255))
                .Column<decimal>("Numeric", column => column.Nullable())
            );

            SchemaBuilder.CreateMapIndexTable(nameof(DateTimeFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(255))
                .Column<string>("ContentPart", column => column.WithLength(255))
                .Column<string>("ContentField", column => column.WithLength(255))
                .Column<DateTime>("DateTime", column => column.Nullable())
            );

            SchemaBuilder.CreateMapIndexTable(nameof(DateFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(255))
                .Column<string>("ContentPart", column => column.WithLength(255))
                .Column<string>("ContentField", column => column.WithLength(255))
                .Column<DateTime>("Date", column => column.Nullable())
            );

            SchemaBuilder.CreateMapIndexTable(nameof(ContentPickerFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(255))
                .Column<string>("ContentPart", column => column.WithLength(255))
                .Column<string>("ContentField", column => column.WithLength(255))
                .Column<string>("ContentItemId", column => column.WithLength(26))
            );

            return 1;
        }
    }
}