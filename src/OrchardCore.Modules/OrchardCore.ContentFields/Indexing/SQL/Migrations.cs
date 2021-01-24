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
            // INFO: The Text Length is now of 766 chars, but this is only used on a new installation.
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
                .CreateIndex("IDX_TextFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_DocumentId_Text",
                    "DocumentId",
                    "Text",
                    "Published",
                    "Latest")
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
                .CreateIndex("IDX_BooleanFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Boolean",
                    "Published",
                    "Latest")
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
                .CreateIndex("IDX_NumericFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_DocumentId_Numeric",
                    "DocumentId",
                    "Numeric",
                    "Published",
                    "Latest")
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
                .CreateIndex("IDX_DateTimeFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_DocumentId_DateTime",
                    "DocumentId",
                    "DateTime",
                    "Published",
                    "Latest")
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
                .CreateIndex("IDX_DateFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_DocumentId_Date",
                    "DocumentId",
                    "ContentType",
                    "Date",
                    "Published",
                    "Latest")
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
                .CreateIndex("IDX_ContentPickerFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_DocumentId_SelectedContentItemId",
                    "DocumentId",
                    "SelectedContentItemId",
                    "Published",
                    "Latest")
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
                .CreateIndex("IDX_TimeFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_DocumentId_Time",
                    "DocumentId",
                    "Time",
                    "Published",
                    "Latest")
            );

            // NOTE: The Url and Text Length has been decreased from 4000 characters to 768.
            // For existing SQL databases update the LinkFieldIndex tables Url and Text column length manually.
            // The BigText and BigUrl columns are new additions so will not be populated until the content item is republished.
            // INFO: The Url and Text Length is now of 766 chars, but this is only used on a new installation.
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
                .CreateIndex("IDX_LinkFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_DocumentId_Url",
                    "DocumentId",
                    "Url",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_DocumentId_Text",
                    "DocumentId",
                    "Text",
                    "Published",
                    "Latest")
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
                .CreateIndex("IDX_HtmlFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<HtmlFieldIndex>(table => table
                .CreateIndex("IDX_HtmlFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.CreateMapIndexTable<MultiTextFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<string>("Value", column => column.Nullable().WithLength(MultiTextFieldIndex.MaxValueSize))
                .Column<string>("BigValue", column => column.Nullable().Unlimited())
            );

            SchemaBuilder.AlterIndexTable<MultiTextFieldIndex>(table => table
                .CreateIndex("IDX_MultiTextFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<MultiTextFieldIndex>(table => table
                .CreateIndex("IDX_MultiTextFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<MultiTextFieldIndex>(table => table
                .CreateIndex("IDX_MultiTextFieldIndex_DocumentId_Value",
                    "DocumentId",
                    "Value",
                    "Published",
                    "Latest")
            );

            // Shortcut other migration steps on new content definition schemas.
            return 4;
        }

        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .AddColumn<string>("BigUrl", column => column.Nullable().Unlimited()));

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .AddColumn<string>("BigText", column => column.Nullable().Unlimited()));

            return 2;
        }

        // This code can be removed in a later version.
        public int UpdateFrom2()
        {
            SchemaBuilder.CreateMapIndexTable<MultiTextFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<string>("Value", column => column.Nullable().WithLength(MultiTextFieldIndex.MaxValueSize))
                .Column<string>("BigValue", column => column.Nullable().Unlimited())
            );

            return 3;
        }

        // This code can be removed in a later version.
        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
                .CreateIndex("IDX_TextFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            // Can't be created on existing databases where the 'Text' may be of 768 chars.
            //SchemaBuilder.AlterIndexTable<TextFieldIndex>(table => table
            //    .CreateIndex("IDX_TextFieldIndex_DocumentId_Text",
            //        "DocumentId",
            //        "Text",
            //        "Published",
            //        "Latest")
            //);

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<BooleanFieldIndex>(table => table
                .CreateIndex("IDX_BooleanFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Boolean",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<NumericFieldIndex>(table => table
                .CreateIndex("IDX_NumericFieldIndex_DocumentId_Numeric",
                    "DocumentId",
                    "Numeric",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateTimeFieldIndex>(table => table
                .CreateIndex("IDX_DateTimeFieldIndex_DocumentId_DateTime",
                    "DocumentId",
                    "DateTime",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<DateFieldIndex>(table => table
                .CreateIndex("IDX_DateFieldIndex_DocumentId_Date",
                    "DocumentId",
                    "Date",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<ContentPickerFieldIndex>(table => table
                .CreateIndex("IDX_ContentPickerFieldIndex_DocumentId_SelectedContentItemId",
                    "DocumentId",
                    "SelectedContentItemId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<TimeFieldIndex>(table => table
                .CreateIndex("IDX_TimeFieldIndex_DocumentId_Time",
                    "DocumentId",
                    "Time",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
                .CreateIndex("IDX_LinkFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            // Can't be created on existing databases where the 'Url' may be of 768 chars.
            //SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
            //    .CreateIndex("IDX_LinkFieldIndex_DocumentId_Url",
            //        "DocumentId",
            //        "Url",
            //        "Published",
            //        "Latest")
            //);

            // Can't be created on existing databases where the 'Text' may be of 768 chars.
            //SchemaBuilder.AlterIndexTable<LinkFieldIndex>(table => table
            //    .CreateIndex("IDX_LinkFieldIndex_DocumentId_Text",
            //        "DocumentId",
            //        "Text",
            //        "Published",
            //        "Latest")
            //);

            SchemaBuilder.AlterIndexTable<HtmlFieldIndex>(table => table
                .CreateIndex("IDX_HtmlFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<HtmlFieldIndex>(table => table
                .CreateIndex("IDX_HtmlFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<MultiTextFieldIndex>(table => table
                .CreateIndex("IDX_MultiTextFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<MultiTextFieldIndex>(table => table
                .CreateIndex("IDX_MultiTextFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<MultiTextFieldIndex>(table => table
                .CreateIndex("IDX_MultiTextFieldIndex_DocumentId_Value",
                    "DocumentId",
                    "Value",
                    "Published",
                    "Latest")
            );

            return 4;
        }
    }
}
