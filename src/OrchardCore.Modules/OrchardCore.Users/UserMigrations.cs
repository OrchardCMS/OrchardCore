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
            .Column<string>("Role", column => column.WithLength(255))
        );

        // First save changes to make sure that user indexes are created before we execute query
        await _session.SaveChangesAsync();

        var users = await _session.Query<User>().ListAsync();

        foreach (var user in users)
        {
            // Saving the user will allow all index providers to run
            _session.Save(user);
        }

        return 1;
    }
}
