using System.Linq;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes
{
    public class UserByClaimIndex : MapIndex
    {
        public string ClaimType { get; set; }

        public string ClaimValue { get; set; }
    }

    public class UserByClaimIndexProvider : IndexProvider<User>
    {
        public override void Describe(DescribeContext<User> context)
        {
            context.For<UserByClaimIndex>()
                .Map(user => user.UserClaims.Select(x => new UserByClaimIndex
                {
                    ClaimType = x.ClaimType,
                    ClaimValue = x.ClaimValue,
                }));
        }
    }
}
