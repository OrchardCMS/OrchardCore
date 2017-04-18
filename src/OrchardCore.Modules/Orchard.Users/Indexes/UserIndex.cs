using Orchard.Users.Models;
using YesSql.Indexes;

namespace Orchard.Users.Indexes
{
    public class UserIndex : MapIndex
    {
        public string NormalizedUserName { get; set; }
        public string NormalizedEmail { get; set; }
    }

    public class UserIndexProvider : IndexProvider<User>
    {
        public override void Describe(DescribeContext<User> context)
        {
            context.For<UserIndex>()
                .Map(user =>
                {
                    return new UserIndex
                    {
                        NormalizedUserName = user.NormalizedUserName,
                        NormalizedEmail = user.NormalizedEmail
                    };
                });
        }
    }
}