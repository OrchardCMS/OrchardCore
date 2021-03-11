using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Sql;

namespace OrchardCore.ContentFields.Indexing.SQL
{
    [Feature("OrchardCore.ContentFields.Indexing.SQL.UserPicker")]
    public class UserPickerMigrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<UserPickerFieldIndex>(table => table
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemVersionId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<string>("ContentPart", column => column.WithLength(ContentItemIndex.MaxContentPartSize))
                .Column<string>("ContentField", column => column.WithLength(ContentItemIndex.MaxContentFieldSize))
                .Column<bool>("Published", column => column.Nullable())
                .Column<bool>("Latest", column => column.Nullable())
                .Column<string>("SelectedUserId")
            );

            SchemaBuilder.AlterIndexTable<UserPickerFieldIndex>(table => table
                .CreateIndex("IDX_UserPickerFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<UserPickerFieldIndex>(table => table
                .CreateIndex("IDX_UserPickerFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<UserPickerFieldIndex>(table => table
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
        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTable<UserPickerFieldIndex>(table => table
                .CreateIndex("IDX_UserPickerFieldIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<UserPickerFieldIndex>(table => table
                .CreateIndex("IDX_UserPickerFieldIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "ContentPart",
                    "ContentField",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<UserPickerFieldIndex>(table => table
                .CreateIndex("IDX_UserPickerFieldIndex_DocumentId_SelectedUserId",
                    "DocumentId",
                    "SelectedUserId",
                    "Published",
                    "Latest")
            );

            return 2;
        }
    }
}
