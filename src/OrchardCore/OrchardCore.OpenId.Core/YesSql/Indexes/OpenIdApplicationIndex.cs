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

    public class OpenIdAppPostLogoutRedirectIndex : ReduceIndex
    {
        public string LogoutRedirectUri { get; set; }
        public int Count { get; set; }
    }

    public class OpenIdApplicationByRedirectUriIndex : ReduceIndex
    {
        public string RedirectUri { get; set; }
        public int Count { get; set; }
    }

    public class OpenIdApplicationByRoleNameIndex : ReduceIndex
    {
        public string RoleName { get; set; }
        public int Count { get; set; }
    }

    public class OpenIdApplicationIndexProvider : IndexProvider<OpenIdApplication>
    {
        public override void Describe(DescribeContext<OpenIdApplication> context)
        {
            context.For<OpenIdApplicationIndex>()
                .Map(application => new OpenIdApplicationIndex
                {
                    ApplicationId = application.ApplicationId,
                    ClientId = application.ClientId
                });

            context.For<OpenIdAppPostLogoutRedirectIndex, string>()
                .Map(application => application.PostLogoutRedirectUris.Select(uri => new OpenIdAppPostLogoutRedirectIndex
                {
                    LogoutRedirectUri = uri,
                    Count = 1
                }))
                .Group(index => index.LogoutRedirectUri)
                .Reduce(group => new OpenIdAppPostLogoutRedirectIndex
                {
                    LogoutRedirectUri = group.Key,
                    Count = group.Sum(x => x.Count)
                })
                .Delete((index, map) =>
                {
                    index.Count -= map.Sum(x => x.Count);
                    return index.Count > 0 ? index : null;
                });

            context.For<OpenIdApplicationByRedirectUriIndex, string>()
                .Map(application => application.RedirectUris.Select(uri => new OpenIdApplicationByRedirectUriIndex
                {
                    RedirectUri = uri,
                    Count = 1
                }))
                .Group(index => index.RedirectUri)
                .Reduce(group => new OpenIdApplicationByRedirectUriIndex
                {
                    RedirectUri = group.Key,
                    Count = group.Sum(x => x.Count)
                })
                .Delete((index, map) =>
                {
                    index.Count -= map.Sum(x => x.Count);
                    return index.Count > 0 ? index : null;
                });

            context.For<OpenIdApplicationByRoleNameIndex, string>()
                .Map(application => application.Roles.Select(role => new OpenIdApplicationByRoleNameIndex
                {
                    RoleName = role,
                    Count = 1
                }))
                .Group(index => index.RoleName)
                .Reduce(group => new OpenIdApplicationByRoleNameIndex
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