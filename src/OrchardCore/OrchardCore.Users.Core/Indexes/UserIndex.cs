using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes
{
    public class UserIndex : MapIndex
    {
        public string DocumentId { get; set; }
        public string NormalizedUserName { get; set; }
        public string NormalizedEmail { get; set; }
        public bool IsEnabled { get; set; }
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
                        NormalizedEmail = user.NormalizedEmail,
                        IsEnabled = user.IsEnabled
                    };
                });
        }
    }
}
