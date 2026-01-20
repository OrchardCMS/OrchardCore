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

    public class OpenIdAppByLogoutUriIndex : ReduceIndex
    {
        public string LogoutRedirectUri { get; set; }
        public int Count { get; set; }
    }

    public class OpenIdAppByRedirectUriIndex : ReduceIndex
    {
        public string RedirectUri { get; set; }
        public int Count { get; set; }
    }

    public class OpenIdAppByRoleNameIndex : ReduceIndex
    {
        public string RoleName { get; set; }
        public int Count { get; set; }
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
                    LogoutRedirectUri = uri,
                    Count = 1
                }))
                .Group(index => index.LogoutRedirectUri)
                .Reduce(group => new OpenIdAppByLogoutUriIndex
                {
                    LogoutRedirectUri = group.Key,
                    Count = group.Sum(x => x.Count)
                })
                .Delete((index, map) =>
                {
                    index.Count -= map.Sum(x => x.Count);
                    return index.Count > 0 ? index : null;
                });

            context.For<OpenIdAppByRedirectUriIndex, string>()
                .Map(application => application.RedirectUris.Select(uri => new OpenIdAppByRedirectUriIndex
                {
                    RedirectUri = uri,
                    Count = 1
                }))
                .Group(index => index.RedirectUri)
                .Reduce(group => new OpenIdAppByRedirectUriIndex
                {
                    RedirectUri = group.Key,
                    Count = group.Sum(x => x.Count)
                })
                .Delete((index, map) =>
                {
                    index.Count -= map.Sum(x => x.Count);
                    return index.Count > 0 ? index : null;
                });

            context.For<OpenIdAppByRoleNameIndex, string>()
                .Map(application => application.Roles.Select(role => new OpenIdAppByRoleNameIndex
                {
                    RoleName = role,
                    Count = 1
                }))
                .Group(index => index.RoleName)
                .Reduce(group => new OpenIdAppByRoleNameIndex
                {
                    RoleName = group.Key,
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
