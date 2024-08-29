using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.ContentFields.Indexing.SQL;

public sealed class Migrations : DataMigration
{
    private readonly ILogger _logger;

    public Migrations(ILogger<Migrations> logger)
    {
        _logger = logger;
    }

    public async Task<int> CreateAsync()
    {
        // NOTE: The Text Length has been decreased from 4000 characters to 768.
        // For existing SQL databases update the TextFieldIndex tables Text column length manually.
        // INFO: The Text Length is now of 766 chars, but this is only used on a new installation.
        await SchemaBuilder.CreateMapIndexTableAsync<TextFieldIndex>(table => table
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

        await SchemaBuilder.AlterIndexTableAsync<TextFieldIndex>(table => table
            .CreateIndex("IDX_TextFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<TextFieldIndex>(table => table
            .CreateIndex("IDX_TextFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + Text (764) + Published and Latest (1) = 767 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<TextFieldIndex>(table => table
            .CreateIndex("IDX_TextFieldIndex_DocumentId_Text",
                "DocumentId",
                "Text(764)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<BooleanFieldIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ContentItemVersionId", column => column.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<bool>("Published", column => column.Nullable())
            .Column<bool>("Latest", column => column.Nullable())
            .Column<bool>("Boolean", column => column.Nullable())
        );

        await SchemaBuilder.AlterIndexTableAsync<BooleanFieldIndex>(table => table
            .CreateIndex("IDX_BooleanFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Boolean, Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<BooleanFieldIndex>(table => table
            .CreateIndex("IDX_BooleanFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Boolean",
                "Published",
                "Latest")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<NumericFieldIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ContentItemVersionId", column => column.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<bool>("Published", column => column.Nullable())
            .Column<bool>("Latest", column => column.Nullable())
            .Column<decimal>("Numeric", column => column.Nullable())
        );

        await SchemaBuilder.AlterIndexTableAsync<NumericFieldIndex>(table => table
            .CreateIndex("IDX_NumericFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<NumericFieldIndex>(table => table
            .CreateIndex("IDX_NumericFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<NumericFieldIndex>(table => table
            .CreateIndex("IDX_NumericFieldIndex_DocumentId_Numeric",
                "DocumentId",
                "Numeric",
                "Published",
                "Latest")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<DateTimeFieldIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ContentItemVersionId", column => column.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<bool>("Published", column => column.Nullable())
            .Column<bool>("Latest", column => column.Nullable())
            .Column<DateTime>("DateTime", column => column.Nullable())
        );

        await SchemaBuilder.AlterIndexTableAsync<DateTimeFieldIndex>(table => table
            .CreateIndex("IDX_DateTimeFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<DateTimeFieldIndex>(table => table
            .CreateIndex("IDX_DateTimeFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<DateTimeFieldIndex>(table => table
            .CreateIndex("IDX_DateTimeFieldIndex_DocumentId_DateTime",
                "DocumentId",
                "DateTime",
                "Published",
                "Latest")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<DateFieldIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ContentItemVersionId", column => column.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<bool>("Published", column => column.Nullable())
            .Column<bool>("Latest", column => column.Nullable())
            .Column<DateTime>("Date", column => column.Nullable())
        );

        await SchemaBuilder.AlterIndexTableAsync<DateFieldIndex>(table => table
            .CreateIndex("IDX_DateFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<DateFieldIndex>(table => table
            .CreateIndex("IDX_DateFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<DateFieldIndex>(table => table
            .CreateIndex("IDX_DateFieldIndex_DocumentId_Date",
                "DocumentId",
                "ContentType",
                "Date",
                "Published",
                "Latest")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<ContentPickerFieldIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ContentItemVersionId", column => column.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<bool>("Published", column => column.Nullable())
            .Column<bool>("Latest", column => column.Nullable())
            .Column<string>("SelectedContentItemId", column => column.WithLength(26))
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentPickerFieldIndex>(table => table
            .CreateIndex("IDX_ContentPickerFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<ContentPickerFieldIndex>(table => table
            .CreateIndex("IDX_ContentPickerFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentPickerFieldIndex>(table => table
            .CreateIndex("IDX_ContentPickerField_DocumentId_SelectedItemId",
                "DocumentId",
                "SelectedContentItemId",
                "Published",
                "Latest")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<TimeFieldIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ContentItemVersionId", column => column.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<bool>("Published", column => column.Nullable())
            .Column<bool>("Latest", column => column.Nullable())
            .Column<TimeSpan>("Time", column => column.Nullable())
        );

        await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
            .CreateIndex("IDX_TimeFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
            .CreateIndex("IDX_TimeFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
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
        await SchemaBuilder.CreateMapIndexTableAsync<LinkFieldIndex>(table => table
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

        await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
            .CreateIndex("IDX_LinkFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );


        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
            .CreateIndex("IDX_LinkFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + Url (764) + Published and Latest (1) = 767 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
            .CreateIndex("IDX_LinkFieldIndex_DocumentId_Url",
                "DocumentId",
                "Url(764)",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + Text (764) + Published and Latest (1) = 767 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
            .CreateIndex("IDX_LinkFieldIndex_DocumentId_Text",
                "DocumentId",
                "Text(764)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<HtmlFieldIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ContentItemVersionId", column => column.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<bool>("Published", column => column.Nullable())
            .Column<bool>("Latest", column => column.Nullable())
            .Column<string>("Html", column => column.Nullable().Unlimited())
        );

        await SchemaBuilder.AlterIndexTableAsync<HtmlFieldIndex>(table => table
            .CreateIndex("IDX_HtmlFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<HtmlFieldIndex>(table => table
            .CreateIndex("IDX_HtmlFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<MultiTextFieldIndex>(table => table
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

        await SchemaBuilder.AlterIndexTableAsync<MultiTextFieldIndex>(table => table
            .CreateIndex("IDX_MultiTextFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<MultiTextFieldIndex>(table => table
            .CreateIndex("IDX_MultiTextFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + Value (764) + Published and Latest (1) = 767 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<MultiTextFieldIndex>(table => table
            .CreateIndex("IDX_MultiTextFieldIndex_DocumentId_Value",
                "DocumentId",
                "Value(764)",
                "Published",
                "Latest")
        );

        // Shortcut other migration steps on new content definition schemas.
        return 5;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
            .AddColumn<string>("BigUrl", column => column.Nullable().Unlimited()));

        await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
            .AddColumn<string>("BigText", column => column.Nullable().Unlimited()));

        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<MultiTextFieldIndex>(table => table
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
    public async Task<int> UpdateFrom3Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<TextFieldIndex>(table => table
            .CreateIndex("IDX_TextFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<TextFieldIndex>(table => table
            .CreateIndex("IDX_TextFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + Text (764) + Published and Latest (1) = 767 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<TextFieldIndex>(table => table
            .CreateIndex("IDX_TextFieldIndex_DocumentId_Text",
                "DocumentId",
                "Text(764)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<BooleanFieldIndex>(table => table
            .CreateIndex("IDX_BooleanFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Boolean, Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<BooleanFieldIndex>(table => table
            .CreateIndex("IDX_BooleanFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Boolean",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<NumericFieldIndex>(table => table
            .CreateIndex("IDX_NumericFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<NumericFieldIndex>(table => table
            .CreateIndex("IDX_NumericFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<NumericFieldIndex>(table => table
            .CreateIndex("IDX_NumericFieldIndex_DocumentId_Numeric",
                "DocumentId",
                "Numeric",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<DateTimeFieldIndex>(table => table
            .CreateIndex("IDX_DateTimeFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<DateTimeFieldIndex>(table => table
            .CreateIndex("IDX_DateTimeFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<DateTimeFieldIndex>(table => table
            .CreateIndex("IDX_DateTimeFieldIndex_DocumentId_DateTime",
                "DocumentId",
                "DateTime",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<DateFieldIndex>(table => table
            .CreateIndex("IDX_DateFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<DateFieldIndex>(table => table
            .CreateIndex("IDX_DateFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<DateFieldIndex>(table => table
            .CreateIndex("IDX_DateFieldIndex_DocumentId_Date",
                "DocumentId",
                "Date",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentPickerFieldIndex>(table => table
            .CreateIndex("IDX_ContentPickerFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<ContentPickerFieldIndex>(table => table
            .CreateIndex("IDX_ContentPickerFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentPickerFieldIndex>(table => table
            .CreateIndex("IDX_ContentPickerField_DocumentId_SelectedItemId",
                "DocumentId",
                "SelectedContentItemId",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
            .CreateIndex("IDX_TimeFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
            .CreateIndex("IDX_TimeFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
            .CreateIndex("IDX_TimeFieldIndex_DocumentId_Time",
                "DocumentId",
                "Time",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
            .CreateIndex("IDX_LinkFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
            .CreateIndex("IDX_LinkFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + Url (764) + Published and Latest (1) = 767 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
            .CreateIndex("IDX_LinkFieldIndex_DocumentId_Url",
                "DocumentId",
                "Url(764)",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + Text (764) + Published and Latest (1) = 767 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
            .CreateIndex("IDX_LinkFieldIndex_DocumentId_Text",
                "DocumentId",
                "Text(764)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<HtmlFieldIndex>(table => table
            .CreateIndex("IDX_HtmlFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<HtmlFieldIndex>(table => table
            .CreateIndex("IDX_HtmlFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<MultiTextFieldIndex>(table => table
            .CreateIndex("IDX_MultiTextFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<MultiTextFieldIndex>(table => table
            .CreateIndex("IDX_MultiTextFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + Value (764) + Published and Latest (1) = 767 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<MultiTextFieldIndex>(table => table
            .CreateIndex("IDX_MultiTextFieldIndex_DocumentId_Value",
                "DocumentId",
                "Value(764)",
                "Published",
                "Latest")
        );

        return 4;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom4Async()
    {
        // Attempts to drop an index that existed only in RC2.
        try
        {
            await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
                .DropIndex("IDX_TimeFieldIndex_Time")
            );
        }
        catch
        {
            _logger.LogWarning("Failed to drop an index that does not exist 'IDX_TimeFieldIndex_Time'");
        }

        await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
            .DropIndex("IDX_TimeFieldIndex_DocumentId_Time")
        );

        // SqLite does not support dropping columns.
        try
        {
            await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
                .DropColumn("Time"));

            await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
                .AddColumn<TimeSpan>("Time", column => column.Nullable()));
        }
        catch
        {
            _logger.LogWarning("Failed to alter 'Time' column. This is not an error when using SqLite");
        }

        await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
            .CreateIndex("IDX_TimeFieldIndex_DocumentId_Time",
                "DocumentId",
                "Time",
                "Published",
                "Latest")
        );

        return 5;
    }
}
