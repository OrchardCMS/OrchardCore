using System.Linq;
using Orchard.OpenId.Models;
using YesSql.Indexes;

namespace Orchard.OpenId.Indexes
{
    public class OpenIdApplicationByRoleNameIndex : ReduceIndex
    {
        public string RoleName { get; set; }
        public int Count { get; set; }
    }

    public class OpenIdApplicationByRoleNameIndexProvider : IndexProvider<OpenIdApplication>
    {
        public override void Describe(DescribeContext<OpenIdApplication> context)
        {
            context.For<OpenIdApplicationByRoleNameIndex, string>()
                .Map(openIdApplication => openIdApplication.RoleNames.Select(x => new OpenIdApplicationByRoleNameIndex
                {
                    RoleName = x,
                    Count = 1
                }))
                .Group(openIdApplication => openIdApplication.RoleName)
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