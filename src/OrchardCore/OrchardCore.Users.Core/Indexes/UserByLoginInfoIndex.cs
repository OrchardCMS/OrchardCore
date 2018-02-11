using System.Linq;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes
{
    public class UserByLoginInfoIndex : ReduceIndex
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public int Count { get; set; }
    }

    public class UserByLoginInfoIndexProvider : IndexProvider<User>
    {
        public override void Describe(DescribeContext<User> context)
        {
            context.For<UserByLoginInfoIndex, string>()
                .Map(user => user.LoginInfos.Select(x => new UserByLoginInfoIndex
                {
                    LoginProvider = x.LoginProvider,
                    ProviderKey = x.ProviderKey,
                    Count = 1
                }))
                .Group(externalLogin => new { externalLogin.LoginProvider, externalLogin.ProviderKey })
                .Reduce(group => new UserByLoginInfoIndex
                {
                    LoginProvider = group.Key.LoginProvider,
                    ProviderKey = group.Key.ProviderKey,
                    Count = group.Sum(x => x.Count)
                })
                .Delete((index, map) =>
                {
                    index.Count -= map.Sum(x => x.Count);
                    return index.Count > 0 ? index : null;
                });
        }
    }
}