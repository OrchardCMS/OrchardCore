using Orchard.DependencyInjection;
using Orchard.OpenId.Models;
using YesSql.Core.Indexes;

namespace Orchard.OpenId.Indexes
{
    public class OpenIdApplicationIndex : MapIndex
    {
        public string ClientId { get; set; }   
        public string LogoutRedirectUri { get; set; }
    }

    public class OpenIdApplicationIndexProvider : IndexProvider<OpenIdApplication>, IDependency
    {
        public override void Describe(DescribeContext<OpenIdApplication> context)
        {
            context.For<OpenIdApplicationIndex>()
                .Map(openIdApplication =>
                {
                    return new OpenIdApplicationIndex
                    {
                        ClientId = openIdApplication.ClientId,
                        LogoutRedirectUri = openIdApplication.LogoutRedirectUri
                    };
                });
        }
    }
}