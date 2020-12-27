using System;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.ContentFields.Indexing.SQL
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            // NOTE: The Text Length has been decreased from 4000 characters to 768.
            // For existing SQL databases update the TextFieldIndex tables Text column length manually.
            SchemaBuilder.CreateMapIndexTable<TextFieldIndex>(table => table
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

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_Text", "Text")
            );

            SchemaBuilder.CreateMapIndexTable<BooleanFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<bool>("Boolean", column => column.Nullable())
            );

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_Boolean", "Boolean")
            );

            SchemaBuilder.CreateMapIndexTable<NumericFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<decimal>("Numeric", column => column.Nullable())
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_Numeric", "Numeric")
            );

            SchemaBuilder.CreateMapIndexTable<DateTimeFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<DateTime>("DateTime", column => column.Nullable())
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_DateTime", "DateTime")
            );

            SchemaBuilder.CreateMapIndexTable<DateFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<DateTime>("Date", column => column.Nullable())
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_Date", "Date")
            );

            SchemaBuilder.CreateMapIndexTable<ContentPickerFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<string>("SelectedContentItemId", column => column.WithLength(26))
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_SelectedContentItemId", "SelectedContentItemId")
            );

            SchemaBuilder.CreateMapIndexTable<TimeFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<DateTime>("Time", column => column.Nullable())
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_Time", "Time")
            );

            // NOTE: The Url and Text Length has been decreased from 4000 characters to 768.
            // For existing SQL databases update the LinkFieldIndex tables Url and Text column length manually.
            // The BigText and BigUrl columns are new additions so will not be populated until the content item is republished.
            SchemaBuilder.CreateMapIndexTable<LinkFieldIndex>(table => table
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

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_Latest", "Latest")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_Url", "Url")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_Text", "Text")
            );

            SchemaBuilder.CreateMapIndexTable<HtmlFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<string>("Html", column => column.Nullable().Unlimited())
            );

            SchemaBuilder.AlterIndexTable<HtmlFieldIndex>(table => table
                .CreateIndex("IDX_HtmlFieldIndex_ContentItemId", "ContentItemId")
            );

            SchemaBuilder.AlterIndexTable<HtmlFieldIndex>(table => table
                .CreateIndex("IDX_HtmlFieldIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterIndexTable<HtmlFieldIndex>(table => table
                .CreateIndex("IDX_HtmlFieldIndex_ContentType", "ContentType")
            );

            SchemaBuilder.AlterIndexTable<HtmlFieldIndex>(table => table
                .CreateIndex("IDX_HtmlFieldIndex_ContentPart", "ContentPart")
            );

            SchemaBuilder.AlterIndexTable<HtmlFieldIndex>(table => table
                .CreateIndex("IDX_HtmlFieldIndex_ContentField", "ContentField")
            );

            SchemaBuilder.AlterIndexTable<HtmlFieldIndex>(table => table
                .CreateIndex("IDX_HtmlFieldIndex_Published", "Published")
            );

            SchemaBuilder.AlterIndexTable<HtmlFieldIndex>(table => table
                .CreateIndex("IDX_HtmlFieldIndex_Latest", "Latest")
            );

            // Return 2 to shorcut migrations on new installations.
            return 2;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .AddColumn<string>("BigUrl", column => column.Nullable().Unlimited()));

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .AddColumn<string>("BigText", column => column.Nullable().Unlimited()));

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateMapIndexTable<MultiSelectFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<string>("Value", column => column.Nullable().WithLength(MultiSelectFieldIndex.MaxValueSize))
                .Column<string>("BigValue", column => column.Nullable().Unlimited())
            );

            // The indexes here represent a change in technique to always include the DocumentId
            SchemaBuilder.AlterIndexTable<MultiSelectFieldIndex>(table => table
                .CreateIndex("IDX_MultiSelectFieldIndex_Ids", "DocumentId", "ContentItemId", "Published", "Latest", "ContentItemVersionId")
            );

            SchemaBuilder.AlterIndexTable<MultiSelectFieldIndex>(table => table
                .CreateIndex("IDX_MultiSelectFieldIndex_Types", "DocumentId", "ContentType", "ContentPart", "ContentField")
            );

            // This must stay under the MySql limits on length for key value indexes.
            SchemaBuilder.AlterIndexTable<MultiSelectFieldIndex>(table => table
                .CreateIndex("IDX_MultiSelectFieldIndex_Value", "DocumentId", "Value")
            );

            return 3;
        }
    }
}
