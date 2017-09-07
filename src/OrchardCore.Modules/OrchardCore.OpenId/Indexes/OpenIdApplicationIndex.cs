using OrchardCore.OpenId.Models;
using YesSql.Indexes;

namespace OrchardCore.OpenId.Indexes
{
    public class OpenIdApplicationIndex : MapIndex
    {
        public string ClientId { get; set; }   
        public string LogoutRedirectUri { get; set; }
        public string RedirectUri { get; set; }
    }

    public class OpenIdApplicationIndexProvider : IndexProvider<OpenIdApplication>
    {
        public override void Describe(DescribeContext<OpenIdApplication> context)
        {
            context.For<OpenIdApplicationIndex>().Map(application =>
            {
                return new OpenIdApplicationIndex
                {
                    ClientId = application.ClientId,
                    LogoutRedirectUri = application.LogoutRedirectUri,
                    RedirectUri = application.RedirectUri
                };
            });
        }
    }
}