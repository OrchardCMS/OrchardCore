using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes
{
    public class UserTokenIndex : MapIndex
    {
        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
    }

    public class UserTokenIndexProvider : IndexProvider<UserToken>
    {
        public override void Describe(DescribeContext<UserToken> context)
        {
            context.For<UserTokenIndex>()
                .Map(userToken =>
                {
                    return new UserTokenIndex
                    {
                        LoginProvider = userToken.LoginProvider,
                        Name = userToken.Name,
                        UserId = userToken.UserId
                    };
                });
        }
    }
}
