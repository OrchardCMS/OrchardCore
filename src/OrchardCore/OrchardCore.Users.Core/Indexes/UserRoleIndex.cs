using System.Linq;
using Microsoft.AspNetCore.Identity;
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
    private readonly ILookupNormalizer _keyNormalizer;

    public UserRoleIndexProvider(ILookupNormalizer keyNormalizer)
    {
        _keyNormalizer = keyNormalizer;
    }

    public override void Describe(DescribeContext<User> context)
    {
        context.For<UserRoleIndex>()
            .Map(user =>
            {
                return user.RoleNames.Distinct()
                    .Select(roleName => new UserRoleIndex()
                    {
                        UserId = user.UserId,
                        Role = _keyNormalizer.NormalizeName(roleName),
                    });
            });
    }
}
