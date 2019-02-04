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
                .Column<string>("ContentType")
                .Column<string>("ContentPart")
                .Column<string>("ContentField")
                .Column<string>("Text")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(BooleanFieldIndex), table => table
                .Column<string>("ContentType")
                .Column<string>("ContentPart")
                .Column<string>("ContentField")
                .Column<bool>("Boolean")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(NumericFieldIndex), table => table
                .Column<string>("ContentType")
                .Column<string>("ContentPart")
                .Column<string>("ContentField")
                .Column<decimal>("Numeric")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(DateTimeFieldIndex), table => table
                .Column<string>("ContentType")
                .Column<string>("ContentPart")
                .Column<string>("ContentField")
                .Column<DateTime>("DateTime")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(DateFieldIndex), table => table
                .Column<string>("ContentType")
                .Column<string>("ContentPart")
                .Column<string>("ContentField")
                .Column<DateTime>("Date")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(ContentPickerFieldIndex), table => table
                .Column<string>("ContentType")
                .Column<string>("ContentPart")
                .Column<string>("ContentField")
                .Column<string>("ContentItemIds")
            );

            return 1;
        }
    }
}