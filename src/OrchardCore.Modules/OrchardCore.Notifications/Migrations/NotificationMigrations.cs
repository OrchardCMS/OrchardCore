using System;
using OrchardCore.Data.Migration;
using OrchardCore.Notifications.Indexes;
using YesSql.Sql;

namespace OrchardCore.Notifications.Migrations;

public class NotificationMigrations : DataMigration
{
    public int Create()
    {
        SchemaBuilder.CreateMapIndexTable<NotificationIndex>(table => table
            .Column<string>("NotificationId", column => column.WithLength(26))
            .Column<string>("UserId", column => column.WithLength(26))
            .Column<bool>("IsRead")
            .Column<DateTime>("ReadAtUtc")
            .Column<DateTime>("CreatedAtUtc")
            .Column<string>("Content", column => column.WithLength(NotificationConstants.NotificationIndexContentLength)),
            collection: NotificationConstants.NotificationCollection
        );

        SchemaBuilder.AlterIndexTable<NotificationIndex>(table => table
            .CreateIndex("IDX_NotificationIndex_DocumentId",
                "DocumentId",
                "NotificationId",
                "UserId",
                "IsRead",
                "CreatedAtUtc",
                "Content"),
            collection: NotificationConstants.NotificationCollection
        );

        return 1;
    }
}
