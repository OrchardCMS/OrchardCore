using System;
using OrchardCore.Data.Migration;
using OrchardCore.Notifications.Indexes;
using YesSql.Sql;

namespace OrchardCore.Notifications.Migrations;

public class NotificationMigrations : DataMigration
{
    public int Create()
    {
        SchemaBuilder.CreateMapIndexTable<WebNotificationMessageIndex>(table => table
            .Column<string>("ContentItemId")
            .Column<string>("UserId")
            .Column<bool>("IsRead")
            .Column<DateTime>("ReadAtUtc")
            .Column<DateTime>("CreatedAtUtc")
            .Column<string>("Subject")
            .Column<string>("Body")
        );

        SchemaBuilder.AlterIndexTable<WebNotificationMessageIndex>(table => table
            .CreateIndex("IDX_WebNotificationMessageIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "UserId",
                "IsRead",
                "CreatedAtUtc")
        );
        /*
        SchemaBuilder.CreateMapIndexTable<WebNotificationIndex>(table => table
                .Column<string>("ContentItemId")
                .Column<string>("UserId")
                .Column<int>("TotalUnread")
                .Column<DateTime>("FirstMessageReceivedAt")
                .Column<DateTime>("LastMessageReceivedAt")
            );

        SchemaBuilder.AlterIndexTable<WebNotificationIndex>(table => table
            .CreateIndex("IDX_WebNotificationIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "UserId",
                "FirstMessageReceivedAt")
        );
        */

        return 1;
    }
}
