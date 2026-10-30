using OrchardCore.Data.Migration;
using OrchardCore.Notifications.Indexes;
using YesSql.Sql;

namespace OrchardCore.Notifications.Migrations;

public sealed class NotificationMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<NotificationIndex>(table => table
            .Column<string>("NotificationId", column => column.WithLength(26))
            .Column<string>("UserId", column => column.WithLength(26))
            .Column<bool>("IsRead")
            .Column<DateTime>("ReadAtUtc")
            .Column<DateTime>("CreatedAtUtc")
            .Column<string>("Content", column => column.WithLength(NotificationConstants.NotificationIndexContentLength))
            .Column<string>("Type", column => column.Nullable().WithLength(NotificationConstants.NotificationTypeLength))
            .Column<string>("ReferenceId", column => column.Nullable().WithLength(NotificationConstants.NotificationReferenceIdLength)),
            collection: NotificationConstants.NotificationCollection
        );

        await SchemaBuilder.AlterIndexTableAsync<NotificationIndex>(table => table
            .CreateIndex("IDX_NotificationIndex_DocumentId",
                "DocumentId",
                "NotificationId",
                "UserId",
                "IsRead",
                "CreatedAtUtc",
                "Content"),
            collection: NotificationConstants.NotificationCollection
        );

        await SchemaBuilder.AlterIndexTableAsync<NotificationIndex>(table => table
            .CreateIndex("IDX_NotificationIndex_UserId",
                "DocumentId",
                "UserId",
                "IsRead",
                "CreatedAtUtc"),
            collection: NotificationConstants.NotificationCollection
        );

        await CreateTypeIndexesAsync();

        return 3;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<NotificationIndex>(table => table
            .CreateIndex("IDX_NotificationIndex_UserId",
                "DocumentId",
                "UserId",
                "IsRead",
                "CreatedAtUtc"),
            collection: NotificationConstants.NotificationCollection
        );

        return 2;
    }

    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<NotificationIndex>(table => table
            .AddColumn<string>("Type", column => column.Nullable().WithLength(NotificationConstants.NotificationTypeLength)),
            collection: NotificationConstants.NotificationCollection
        );

        await SchemaBuilder.AlterIndexTableAsync<NotificationIndex>(table => table
            .AddColumn<string>("ReferenceId", column => column.Nullable().WithLength(NotificationConstants.NotificationReferenceIdLength)),
            collection: NotificationConstants.NotificationCollection
        );

        await CreateTypeIndexesAsync();

        return 3;
    }

    private async Task CreateTypeIndexesAsync()
    {
        // Lists the notifications of a user by type in the notification center.
        await SchemaBuilder.AlterIndexTableAsync<NotificationIndex>(table => table
            .CreateIndex("IDX_NotificationIndex_UserId_Type",
                "DocumentId",
                "UserId",
                "Type",
                "IsRead",
                "CreatedAtUtc"),
            collection: NotificationConstants.NotificationCollection
        );

        // Finds the notifications of a record, see 'INotificationManager'.
        await SchemaBuilder.AlterIndexTableAsync<NotificationIndex>(table => table
            .CreateIndex("IDX_NotificationIndex_Type_ReferenceId",
                "DocumentId",
                "Type",
                "ReferenceId",
                "UserId"),
            collection: NotificationConstants.NotificationCollection
        );
    }
}
