using System;
using OrchardCore.Data.Migration;
using OrchardCore.Notifications.Indexes;
using YesSql.Sql;

namespace OrchardCore.Notifications.Migrations;

public class NotificationMigrations : DataMigration
{
    public int Create()
    {
        SchemaBuilder.CreateMapIndexTable<WebNotificationIndex>(table => table
            .Column<string>("NotificationId")
            .Column<string>("UserId")
            .Column<bool>("IsRead")
            .Column<DateTime>("ReadAtUtc")
            .Column<DateTime>("CreatedAtUtc")
            .Column<string>("Content", column => column.WithLength(NotificationConstants.WebNotificationIndexContentLength))
            , collection: NotificationConstants.NotificationCollection
        );

        SchemaBuilder.AlterIndexTable<WebNotificationIndex>(table => table
            .CreateIndex("IDX_WebNotificationIndex_DocumentId",
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
