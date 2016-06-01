using Orchard.DependencyInjection;
using Orchard.Users.Models;
using YesSql.Core.Indexes;

namespace Orchard.Users.Indexes
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