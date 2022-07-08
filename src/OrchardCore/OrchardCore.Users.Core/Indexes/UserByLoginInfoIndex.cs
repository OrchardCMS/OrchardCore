using System.Linq;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes
{
    public class UserByLoginInfoIndex : MapIndex
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
    }

    public class UserByLoginInfoIndexProvider : IndexProvider<User>
    {
        public override void Describe(DescribeContext<User> context)
        {
            context.For<UserByLoginInfoIndex>()
                .Map(user => user.LoginInfos.Select(x => new UserByLoginInfoIndex
                {
                    LoginProvider = x.LoginProvider,
                    ProviderKey = x.ProviderKey,
                }));
        }
    }
}
