using System;
using OrchardCore.Data.Migration;
using OrchardCore.ContentFields.Indexing;
using OrchardCore.ContentManagement.Records;

namespace OrchardCore.ContentFields
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(TextFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published")
                .Column<bool>("Latest")
                .Column<string>("Text", column => column.Nullable().WithLength(4000))
                .Column<string>("BigText", column => column.Nullable().Unlimited())
            );
             
            SchemaBuilder.AlterTable(nameof(TextFieldIndex), table => table
                .CreateIndex("IDX_TextFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterTable(nameof(TextFieldIndex), table => table
                .CreateIndex("IDX_TextFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterTable(nameof(TextFieldIndex), table => table
                .CreateIndex("IDX_TextFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(TextFieldIndex), table => table
                .CreateIndex("IDX_TextFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterTable(nameof(TextFieldIndex), table => table
                .CreateIndex("IDX_TextFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterTable(nameof(TextFieldIndex), table => table
                .CreateIndex("IDX_TextFieldIndex_Text", "Text")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(BooleanFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published")
                .Column<bool>("Latest")
                .Column<bool>("Boolean", column => column.Nullable())
            );

            SchemaBuilder.AlterTable(nameof(BooleanFieldIndex), table => table
                .CreateIndex("IDX_BooleanFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterTable(nameof(BooleanFieldIndex), table => table
                .CreateIndex("IDX_BooleanFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterTable(nameof(BooleanFieldIndex), table => table
                .CreateIndex("IDX_BooleanFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(BooleanFieldIndex), table => table
                .CreateIndex("IDX_BooleanFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterTable(nameof(BooleanFieldIndex), table => table
                .CreateIndex("IDX_BooleanFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterTable(nameof(BooleanFieldIndex), table => table
                .CreateIndex("IDX_BooleanFieldIndex_Boolean", "Boolean")
            );
            
            SchemaBuilder.CreateMapIndexTable(nameof(NumericFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published")
                .Column<bool>("Latest")
                .Column<decimal>("Numeric", column => column.Nullable())
            );

            SchemaBuilder.AlterTable(nameof(NumericFieldIndex), table => table
                .CreateIndex("IDX_NumericFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterTable(nameof(NumericFieldIndex), table => table
                .CreateIndex("IDX_NumericFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterTable(nameof(NumericFieldIndex), table => table
                .CreateIndex("IDX_NumericFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(NumericFieldIndex), table => table
                .CreateIndex("IDX_NumericFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterTable(nameof(NumericFieldIndex), table => table
                .CreateIndex("IDX_NumericFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterTable(nameof(NumericFieldIndex), table => table
                .CreateIndex("IDX_NumericFieldIndex_Numeric", "Numeric")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(DateTimeFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published")
                .Column<bool>("Latest")
                .Column<DateTime>("DateTime", column => column.Nullable())
            );

            SchemaBuilder.AlterTable(nameof(DateTimeFieldIndex), table => table
                .CreateIndex("IDX_DateTimeFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterTable(nameof(DateTimeFieldIndex), table => table
                .CreateIndex("IDX_DateTimeFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterTable(nameof(DateTimeFieldIndex), table => table
                .CreateIndex("IDX_DateTimeFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(DateTimeFieldIndex), table => table
                .CreateIndex("IDX_DateTimeFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterTable(nameof(DateTimeFieldIndex), table => table
                .CreateIndex("IDX_DateTimeFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterTable(nameof(DateTimeFieldIndex), table => table
                .CreateIndex("IDX_DateTimeFieldIndex_DateTime", "DateTime")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(DateFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published")
                .Column<bool>("Latest")
                .Column<DateTime>("Date", column => column.Nullable())
            );

            SchemaBuilder.AlterTable(nameof(DateFieldIndex), table => table
                .CreateIndex("IDX_DateFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterTable(nameof(DateFieldIndex), table => table
                .CreateIndex("IDX_DateFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterTable(nameof(DateFieldIndex), table => table
                .CreateIndex("IDX_DateFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(DateFieldIndex), table => table
                .CreateIndex("IDX_DateFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterTable(nameof(DateFieldIndex), table => table
                .CreateIndex("IDX_DateFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterTable(nameof(DateFieldIndex), table => table
                .CreateIndex("IDX_DateFieldIndex_Date", "Date")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(ContentPickerFieldIndex), table => table
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published")
                .Column<bool>("Latest")
                .Column<string>("ContentItemId", column => column.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(ContentPickerFieldIndex), table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterTable(nameof(ContentPickerFieldIndex), table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterTable(nameof(ContentPickerFieldIndex), table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(ContentPickerFieldIndex), table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterTable(nameof(ContentPickerFieldIndex), table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterTable(nameof(ContentPickerFieldIndex), table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentItemId", "ContentItemId")
            );

            return 1;
        }
    }
}