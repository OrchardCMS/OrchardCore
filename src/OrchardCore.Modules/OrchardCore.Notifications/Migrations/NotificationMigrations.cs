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
            .Column<string>("ContentItemId")
            .Column<string>("UserId")
            .Column<bool>("IsRead")
            .Column<DateTime>("ReadAtUtc")
            .Column<DateTime>("CreatedAtUtc")
        );

        SchemaBuilder.AlterIndexTable<WebNotificationIndex>(table => table
            .CreateIndex("IDX_WebNotificationIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "UserId",
                "IsRead",
                "CreatedAtUtc")
        );

        return 1;
    }
}
