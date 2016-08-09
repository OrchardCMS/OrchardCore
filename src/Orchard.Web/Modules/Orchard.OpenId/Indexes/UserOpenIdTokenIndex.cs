using Orchard.DependencyInjection;
using Orchard.OpenId.Models;
using YesSql.Core.Indexes;

namespace Orchard.OpenId.Indexes
{
    public class UserOpenIdTokenIndex : MapIndex
    {
        public int UserId { get; set; }   
    }

    public class UserOpenIdTokenIndexProvider : IndexProvider<UserOpenIdToken>, IDependency
    {
        public override void Describe(DescribeContext<UserOpenIdToken> context)
        {
            context.For<UserOpenIdTokenIndex>()
                .Map(userOpenIdToken =>
                {
                    return new UserOpenIdTokenIndex
                    {
                        UserId = userOpenIdToken.UserId
                    };
                });
        }
    }
}