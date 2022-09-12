using System.Collections.Generic;
using System.Linq;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes;

public class UserRoleIndex : MapIndex
{
    public string UserId { get; set; }

    public string Role { get; set; }
}

public class UserRoleIndexProvider : IndexProvider<User>
{
    public override void Describe(DescribeContext<User> context)
    {
        context.For<UserRoleIndex>()
            .Map(user =>
            {
                var indexes = new List<UserRoleIndex>();

                foreach (var role in user.RoleNames.Distinct())
                {
                    indexes.Add(new UserRoleIndex()
                    {
                        UserId = user.UserId,
                        Role = role,
                    });
                }

                return indexes;
            });
    }

}
