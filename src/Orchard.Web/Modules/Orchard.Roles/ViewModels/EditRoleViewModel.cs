using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Roles.ViewModels
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
