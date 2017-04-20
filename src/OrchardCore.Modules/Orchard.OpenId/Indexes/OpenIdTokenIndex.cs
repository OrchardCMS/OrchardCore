using Orchard.OpenId.Models;
using YesSql.Indexes;

namespace Orchard.OpenId.Indexes
{
    public class OpenIdTokenIndex : MapIndex
    {
        public int AppId { get; set; }
        public string Subject { get; set; }
        public int TokenId { get; set; }
    }

    public class OpenIdTokenIndexProvider : IndexProvider<OpenIdToken>
    {
        public override void Describe(DescribeContext<OpenIdToken> context)
        {
            context.For<OpenIdTokenIndex>().Map(token =>
            {
                return new OpenIdTokenIndex
                {
                    AppId = token.AppId,
                    Subject = token.Subject,
                    TokenId = token.Id
                };
            });
        }
    }
}