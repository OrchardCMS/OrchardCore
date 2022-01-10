using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.ContentManagement;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using OrchardCore.Users;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentFields.Services
{
    public class DefaultUserPickerResultProvider : IUserPickerResultProvider
    {
        private readonly IContentManager _contentManager;
        private readonly IRoleService _roleService;
        private readonly RoleManager<IRole> _roleManager;
        private readonly UserManager<IUser> _userManager;
        private readonly ISession _session;

        public DefaultUserPickerResultProvider(
            IContentManager contentManager,
            IRoleService roleService,
            RoleManager<IRole> roleManager,
            UserManager<IUser> userManager,
            ISession session)
        {
            _contentManager = contentManager;
            _roleService = roleService;
            _roleManager = roleManager;
            _userManager = userManager;
            _session = session;
        }

        public string Name => "Default";

        public async Task<IEnumerable<UserPickerResult>> Search(UserPickerSearchContext searchContext)
        {
            var query = _session.Query<User>();

            if (!searchContext.DisplayAllUsers)
            {
                var roles = searchContext.Roles.Select(x => _roleManager.NormalizeKey(x));
                query.With<UserByRoleNameIndex>(x => x.RoleName.IsIn(roles));
            }

            if (!string.IsNullOrEmpty(searchContext.Query))
            {
                query.With<UserIndex>(x => x.NormalizedUserName.Contains(_userManager.NormalizeName(searchContext.Query)));
            }

            var users = await query.Take(50).ListAsync();

            var results = new List<UserPickerResult>();

            foreach (var user in users)
            {
                results.Add(new UserPickerResult
                {
                    UserId = user.UserId,
                    DisplayText = user.UserName,
                    IsEnabled = user.IsEnabled
                });
            }

            return results.OrderBy(x => x.DisplayText);
        }
    }
}
