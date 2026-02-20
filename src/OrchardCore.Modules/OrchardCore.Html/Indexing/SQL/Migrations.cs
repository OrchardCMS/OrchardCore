using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.Html.Indexing.SQL;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
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

        return 2;
    }
}
