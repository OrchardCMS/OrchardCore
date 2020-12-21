using System.Linq;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes
{
    public class UserRoleNameIndex : MapIndex
    {
        public string RoleName { get; set; }
    }

    public class UserRoleNameIndexProvider : IndexProvider<User>
    {
        private readonly ILookupNormalizer _keyNormalizer;

        public UserRoleNameIndexProvider(ILookupNormalizer keyNormalizer)
        {
            _keyNormalizer = keyNormalizer;
        }

        public override void Describe(DescribeContext<User> context)
        {
            context.For<UserRoleNameIndex, string>()
                .Map(user => user.RoleNames.Select(x => new UserRoleNameIndex
                {
                    RoleName = NormalizeKey(x)
                }));
        }

        private string NormalizeKey(string key)
        {
            return _keyNormalizer == null ? key : _keyNormalizer.NormalizeName(key);
        }
    }
}
