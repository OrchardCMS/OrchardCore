using System.Linq;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes
{
    public class UserByRoleNameIndex : ReduceIndex
    {
        public string RoleName { get; set; }
        public int Count { get; set; }
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
                                Count = 1
                            }
                        };
                    }

                    return user.RoleNames.Select(x => new UserByRoleNameIndex
                    {
                        RoleName = NormalizeKey(x),
                        Count = 1
                    });
                })
                .Group(index => index.RoleName)
                .Reduce(group => new UserByRoleNameIndex
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

        private string NormalizeKey(string key)
        {
            return _keyNormalizer.NormalizeName(key);
        }
    }
}
