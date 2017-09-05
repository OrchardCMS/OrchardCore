using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.ViewModels
{
    public class EditRoleViewModel
    {
        public string Name { get; set; }
        public IDictionary<string, IEnumerable<Permission>> RoleCategoryPermissions { get; set; }
        public IEnumerable<string> EffectivePermissions { get; set; }

        [BindNever]
        public Role Role { get; set; }
    }
}
