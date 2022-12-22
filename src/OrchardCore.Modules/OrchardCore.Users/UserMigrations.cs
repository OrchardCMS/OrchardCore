using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;
using YesSql.Sql;

namespace OrchardCore.Users;

public class UserMigrations : DataMigration
{
    public int Create()
    {
        SchemaBuilder.CreateMapIndexTable<UserRoleIndex>(table => table
            .Column<string>("UserId", column => column.WithLength(26))
            .Column<string>("Role", column => column.WithLength(255))
        );

        SchemaBuilder.AlterIndexTable<UserRoleIndex>(table => table
            .CreateIndex("IDX_UserRoleIndex_DocumentId",
                "DocumentId",
                "UserId",
                "Role")
        );

        ShellScope.AddDeferredTask(async scope =>
        {
            var counter = 0;
            var batchSize = 500;

            var session = scope.ServiceProvider.GetRequiredService<ISession>();

            while (true)
            {
                var users = await session.Query<User, UserIndex>()
                .OrderBy(x => x.DocumentId)
                .Skip(counter++ * batchSize)
                .Take(batchSize)
                .ListAsync();

                var totalUsers = users.Count();

                if (totalUsers > 0)
                {
                    foreach (var user in users)
                    {
                        // Saving the user will allow all index providers to run.
                        session.Save(user);
                    }

                    await session.SaveChangesAsync();
                }


                if (totalUsers < batchSize)
                {
                    // If there are less users in the database than the batchSize, nothing left to do.
                    break;
                }
            }
        });

        return 1;
    }
}
