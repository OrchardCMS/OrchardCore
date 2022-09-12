using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Users;

public class UserMigrations : DataMigration
{
    private readonly ISession _session;

    public UserMigrations(ISession session)
    {
        _session = session;
    }

    public async Task<int> CreateAsync()
    {
        SchemaBuilder.CreateMapIndexTable<UserRoleIndex>(table => table
            .Column<string>("UserId", column => column.WithLength(26))
            .Column<string>("Role")
        );

        // first save exting changes to make sure that the index is created before we populate it
        await _session.SaveChangesAsync();

        var users = await _session.Query<User>().ListAsync();

        foreach (var user in users)
        {
            // this will force the index provider to update the indexes
            _session.Save(user);
        }

        return 1;
    }
}
