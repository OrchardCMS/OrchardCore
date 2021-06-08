using System.Linq;
using OrchardCore.OpenId.YesSql.Models;
using YesSql.Indexes;

namespace OrchardCore.OpenId.YesSql.Indexes
{
    public class OpenIdApplicationIndex : MapIndex
    {
        public string ApplicationId { get; set; }
        public string ClientId { get; set; }
    }

    public class OpenIdAppByLogoutUriIndex : MapIndex
    {
        public string LogoutRedirectUri { get; set; }
    }

    public class OpenIdAppByRedirectUriIndex : MapIndex
    {
        public string RedirectUri { get; set; }
    }

    public class OpenIdAppByRoleNameIndex : MapIndex
    {
        public string RoleName { get; set; }
    }

    public class OpenIdApplicationIndexProvider : IndexProvider<OpenIdApplication>
    {
        private const string OpenIdCollection = OpenIdApplication.OpenIdCollection;

        public OpenIdApplicationIndexProvider()
            => CollectionName = OpenIdCollection;

        public override void Describe(DescribeContext<OpenIdApplication> context)
        {
            context.For<OpenIdApplicationIndex>()
                .Map(application => new OpenIdApplicationIndex
                {
                    ApplicationId = application.ApplicationId,
                    ClientId = application.ClientId
                });

            context.For<OpenIdAppByLogoutUriIndex, string>()
                .Map(application => application.PostLogoutRedirectUris.Select(uri => new OpenIdAppByLogoutUriIndex
                {
                    LogoutRedirectUri = uri
                }));

            context.For<OpenIdAppByRedirectUriIndex, string>()
                .Map(application => application.RedirectUris.Select(uri => new OpenIdAppByRedirectUriIndex
                {
                    RedirectUri = uri
                }));

            context.For<OpenIdAppByRoleNameIndex, string>()
                .Map(application => application.Roles.Select(role => new OpenIdAppByRoleNameIndex
                {
                    RoleName = role
                }));
        }
    }
}
