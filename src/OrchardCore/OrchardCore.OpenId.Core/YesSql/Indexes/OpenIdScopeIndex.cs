using System.Linq;
using OrchardCore.OpenId.YesSql.Models;
using YesSql.Indexes;

namespace OrchardCore.OpenId.YesSql.Indexes
{
    public class OpenIdScopeIndex : MapIndex
    {
        public string Name { get; set; }
        public string ScopeId { get; set; }
    }

    public class OpenIdScopeByResourceIndex : ReduceIndex
    {
        public string Resource { get; set; }
        public int Count { get; set; }
    }

    public class OpenIdScopeIndexProvider : IndexProvider<OpenIdScope>
    {
        private const string OpenIdCollection = OpenIdScope.OpenIdCollection;

        public OpenIdScopeIndexProvider()
            => CollectionName = OpenIdCollection;

        public override void Describe(DescribeContext<OpenIdScope> context)
        {
            context.For<OpenIdScopeIndex>()
                .Map(scope => new OpenIdScopeIndex
                {
                    Name = scope.Name,
                    ScopeId = scope.ScopeId
                });

            context.For<OpenIdScopeByResourceIndex, string>()
                .Map(scope => scope.Resources.Select(resource => new OpenIdScopeByResourceIndex
                {
                    Resource = resource,
                    Count = 1
                }))
                .Group(index => index.Resource)
                .Reduce(group => new OpenIdScopeByResourceIndex
                {
                    Resource = group.Key,
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
