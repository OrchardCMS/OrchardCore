using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.ContentFields.Indexing.SQL;

public sealed class UserPickerMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<UserPickerFieldIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("ContentItemVersionId", column => column.WithLength(26))
            .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
            .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
            .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
            .Column<bool>("Published", column => column.Nullable())
            .Column<bool>("Latest", column => column.Nullable())
            .Column<string>("SelectedUserId")
        );

        await SchemaBuilder.AlterIndexTableAsync<UserPickerFieldIndex>(table => table
            .CreateIndex("IDX_UserPickerFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<UserPickerFieldIndex>(table => table
            .CreateIndex("IDX_UserPickerFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<UserPickerFieldIndex>(table => table
            .CreateIndex("IDX_UserPickerFieldIndex_DocumentId_SelectedUserId",
                "DocumentId",
                "SelectedUserId",
                "Published",
                "Latest")
        );

        // Shortcut other migration steps on new content definition schemas.
        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<UserPickerFieldIndex>(table => table
            .CreateIndex("IDX_UserPickerFieldIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "ContentItemVersionId",
                "Published",
                "Latest")
        );

        // The index in MySQL can accommodate up to 768 characters or 3072 bytes.
        // DocumentId (2) + ContentType (254) + ContentPart (254) + ContentField (254) + Published and Latest (1) = 765 (< 768).
        await SchemaBuilder.AlterIndexTableAsync<UserPickerFieldIndex>(table => table
            .CreateIndex("IDX_UserPickerFieldIndex_DocumentId_ContentType",
                "DocumentId",
                "ContentType(254)",
                "ContentPart(254)",
                "ContentField(254)",
                "Published",
                "Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<UserPickerFieldIndex>(table => table
            .CreateIndex("IDX_UserPickerFieldIndex_DocumentId_SelectedUserId",
                "DocumentId",
                "SelectedUserId",
                "Published",
                "Latest")
        );

        return 2;
    }
}
