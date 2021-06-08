using System.Linq;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes
{
    public class UserByRoleNameIndex : MapIndex
    {
        public string RoleName { get; set; }
    }

    public class UserByRoleNameIndexProvider : IndexProvider<User>
    {
        private readonly ILookupNormalizer _keyNormalizer;

        public UserByRoleNameIndexProvider(ILookupNormalizer keyNormalizer)
        {
            _keyNormalizer = keyNormalizer;
        }

        public override void Describe(DescribeContext<User> context)
        {
            context.For<UserByRoleNameIndex, string>()
                .Map(user => 
                {
                    // Include a marker that the user does not have any roles, i.e. is Authenticated only.
                    if (!user.RoleNames.Any())
                    {
                        return new UserByRoleNameIndex[]
                        {
                            new UserByRoleNameIndex
                            {
                                RoleName = NormalizeKey("Authenticated"),
                            }
                        };
                    }

                    return user.RoleNames.Select(x => new UserByRoleNameIndex
                    {
                        RoleName = NormalizeKey(x),
                    });
                });
        }

        private string NormalizeKey(string key)
        {
            return _keyNormalizer.NormalizeName(key);
        }
    }
}
