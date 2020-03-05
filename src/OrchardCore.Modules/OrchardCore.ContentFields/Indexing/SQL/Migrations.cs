using System;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;

namespace OrchardCore.ContentFields.Indexing.SQL
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            // NOTE: The Text Length has been decreased from 4000 characters to 768.
            // For existing SQL databases update the TextFieldIndex tables Text column length manually.
            SchemaBuilder.CreateMapIndexTable(nameof(TextFieldIndex), table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<string>("Text", column => column.Nullable().WithLength(TextFieldIndex.MaxTextSize))
                .Column<string>("BigText", column => column.Nullable().Unlimited())
            );

            SchemaBuilder.AlterTable(nameof(TextFieldIndex), table => table
                .CreateIndex("IDX_TextFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(TextFieldIndex), table => table
                .CreateIndex("IDX_TextFieldIndex_ContentItemVersionId", "ContentItemVersionId")
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
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<bool>("Boolean", column => column.Nullable())
            );

            SchemaBuilder.AlterTable(nameof(BooleanFieldIndex), table => table
                .CreateIndex("IDX_BooleanFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(BooleanFieldIndex), table => table
                .CreateIndex("IDX_BooleanFieldIndex_ContentItemVersionId", "ContentItemVersionId")
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
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<decimal>("Numeric", column => column.Nullable())
            );

            SchemaBuilder.AlterTable(nameof(NumericFieldIndex), table => table
                .CreateIndex("IDX_NumericFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(NumericFieldIndex), table => table
                .CreateIndex("IDX_NumericFieldIndex_ContentItemVersionId", "ContentItemVersionId")
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
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<DateTime>("DateTime", column => column.Nullable())
            );

            SchemaBuilder.AlterTable(nameof(DateTimeFieldIndex), table => table
                .CreateIndex("IDX_DateTimeFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(DateTimeFieldIndex), table => table
                .CreateIndex("IDX_DateTimeFieldIndex_ContentItemVersionId", "ContentItemVersionId")
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
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<DateTime>("Date", column => column.Nullable())
            );

            SchemaBuilder.AlterTable(nameof(DateFieldIndex), table => table
                .CreateIndex("IDX_DateFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(DateFieldIndex), table => table
                .CreateIndex("IDX_DateFieldIndex_ContentItemVersionId", "ContentItemVersionId")
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
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<string>("SelectedContentItemId", column => column.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(ContentPickerFieldIndex), table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(ContentPickerFieldIndex), table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentItemVersionId", "ContentItemVersionId")
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
                .CreateIndex("IDX_ContentPickerFieldIndex_SelectedContentItemId", "SelectedContentItemId")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(TimeFieldIndex), table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<DateTime>("Time", column => column.Nullable())
            );

            SchemaBuilder.AlterTable(nameof(TimeFieldIndex), table => table
                .CreateIndex("IDX_TimeFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(TimeFieldIndex), table => table
                .CreateIndex("IDX_TimeFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterTable(nameof(TimeFieldIndex), table => table
                .CreateIndex("IDX_TimeFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterTable(nameof(TimeFieldIndex), table => table
                .CreateIndex("IDX_TimeFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterTable(nameof(TimeFieldIndex), table => table
                .CreateIndex("IDX_TimeFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(TimeFieldIndex), table => table
                .CreateIndex("IDX_TimeFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterTable(nameof(TimeFieldIndex), table => table
                .CreateIndex("IDX_TimeFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterTable(nameof(TimeFieldIndex), table => table
                .CreateIndex("IDX_TimeFieldIndex_Time", "Time")
            );

            // NOTE: The Url and Text Length has been decreased from 4000 characters to 768.
            // For existing SQL databases update the LinkFieldIndex tables Url and Text column length manually.
            // The BigText and BigUrl columns are new additions so will not be populated until the content item is republished.
            SchemaBuilder.CreateMapIndexTable(nameof(LinkFieldIndex), table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<string>("Url", column => column.Nullable().WithLength(LinkFieldIndex.MaxUrlSize))
                .Column<string>("BigUrl", column => column.Nullable().Unlimited())
                .Column<string>("Text", column => column.Nullable().WithLength(LinkFieldIndex.MaxTextSize))
                .Column<string>("BigText", column => column.Nullable().Unlimited())
            );

            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .CreateIndex("IDX_LinkFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .CreateIndex("IDX_LinkFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .CreateIndex("IDX_LinkFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .CreateIndex("IDX_LinkFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .CreateIndex("IDX_LinkFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .CreateIndex("IDX_LinkFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .CreateIndex("IDX_LinkFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .CreateIndex("IDX_LinkFieldIndex_Url", "Url")
            );

            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .CreateIndex("IDX_LinkFieldIndex_Text", "Text")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(HtmlFieldIndex), table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<string>("Html", column => column.Nullable().Unlimited())
            );

            SchemaBuilder.AlterTable(nameof(HtmlFieldIndex), table => table
                .CreateIndex("IDX_HtmlFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(HtmlFieldIndex), table => table
                .CreateIndex("IDX_HtmlFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterTable(nameof(HtmlFieldIndex), table => table
                .CreateIndex("IDX_HtmlFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterTable(nameof(HtmlFieldIndex), table => table
                .CreateIndex("IDX_HtmlFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterTable(nameof(HtmlFieldIndex), table => table
                .CreateIndex("IDX_HtmlFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(HtmlFieldIndex), table => table
                .CreateIndex("IDX_HtmlFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterTable(nameof(HtmlFieldIndex), table => table
                .CreateIndex("IDX_HtmlFieldIndex_Latest", "Latest")
            );

            // Return 2 to shorcut migrations on new installations.
            return 2;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .AddColumn<string>("BigUrl", column => column.Nullable().Unlimited()));

            SchemaBuilder.AlterTable(nameof(LinkFieldIndex), table => table
                .AddColumn<string>("BigText", column => column.Nullable().Unlimited()));

            return 2;
        }
    }
}
