using System;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.ContentFields.Indexing.SQL
{
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

            SchemaBuilder.AlterTable(nameof(UserPickerFieldIndex), table => table
                .CreateIndex("IDX_UserPickerFieldIndex_ContentItemId", "DocumentId", "ContentItemId")
            );

            SchemaBuilder.AlterTable(nameof(UserPickerFieldIndex), table => table
                .CreateIndex("IDX_UserPickerFieldIndex_ContentItemVersionId", "DocumentId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterTable(nameof(UserPickerFieldIndex), table => table
                .CreateIndex("IDX_UserPickerFieldIndex_ContentType", "DocumentId", "ContentType")
            );

            SchemaBuilder.AlterTable(nameof(UserPickerFieldIndex), table => table
                .CreateIndex("IDX_UserPickerFieldIndex_ContentPart", "DocumentId", "ContentPart")
            );

            SchemaBuilder.AlterTable(nameof(UserPickerFieldIndex), table => table
                .CreateIndex("IDX_UserPickerFieldIndex_ContentField", "DocumentId", "ContentField")
            );

            SchemaBuilder.AlterTable(nameof(UserPickerFieldIndex), table => table
                .CreateIndex("IDX_UserPickerFieldIndex_Published", "DocumentId", "Published")
            );

            SchemaBuilder.AlterTable(nameof(UserPickerFieldIndex), table => table
                .CreateIndex("IDX_UserPickerFieldIndex_Latest", "DocumentId", "Latest")
            );

            SchemaBuilder.AlterTable(nameof(UserPickerFieldIndex), table => table
                .CreateIndex("IDX_UserPickerFieldIndex_SelectedUserId", "DocumentId", "SelectedUserId")
            );

            return 1;
        }
    }
}
