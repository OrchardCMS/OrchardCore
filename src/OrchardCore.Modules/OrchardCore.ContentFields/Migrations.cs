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
                .Column<string>("FieldName")
                .Column<string>("Text")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(BooleanFieldIndex), table => table
                .Column<string>("FieldName")
                .Column<bool>("Boolean")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(NumericFieldIndex), table => table
                .Column<string>("FieldName")
                .Column<decimal>("Numeric")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(DateTimeFieldIndex), table => table
                .Column<string>("FieldName")
                .Column<DateTime>("DateTime")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(DateFieldIndex), table => table
                .Column<string>("FieldName")
                .Column<DateTime>("Date")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(ContentPickerFieldIndex), table => table
                .Column<string>("FieldName")
                .Column<string>("ContentItemIds")
            );

            return 1;
        }
    }
}