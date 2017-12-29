using OrchardCore.OpenId.YesSql.Models;
using YesSql.Indexes;

namespace OrchardCore.OpenId.YesSql.Indexes
{
    public class OpenIdScopeIndex : MapIndex
    {
        public string Name { get; set; }
        public string ScopeId { get; set; }
    }

    public class OpenIdScopeIndexProvider : IndexProvider<OpenIdScope>
    {
        public override void Describe(DescribeContext<OpenIdScope> context)
        {
            context.For<OpenIdScopeIndex>()
                .Map(scope => new OpenIdScopeIndex
                {
                    Name = scope.Name,
                    ScopeId = scope.ScopeId
                });
        }
    }
}