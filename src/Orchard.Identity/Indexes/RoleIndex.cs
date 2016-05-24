using Orchard.DependencyInjection;
using YesSql.Core.Indexes;

namespace Orchard.Identity.Indexes
{
    public class RoleIndex : MapIndex
    {
        public string NormalizedRoleName { get; set; }
    }

    public class RoleIndexProvider : IndexProvider<Role>, IDependency
    {
        public override void Describe(DescribeContext<Role> context)
        {
            context.For<RoleIndex>()
                .Map(role =>
                {
                    return new RoleIndex
                    {
                        NormalizedRoleName = role.NormalizedRoleName
                    };
                });
        }
    }
}