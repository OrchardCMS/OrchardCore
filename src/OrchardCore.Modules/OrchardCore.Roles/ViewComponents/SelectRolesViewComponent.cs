using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles.ViewComponents
{
    public class SelectRolesViewComponent : ViewComponent
    {
        private readonly IRoleService _roleService;

        public SelectRolesViewComponent(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<IViewComponentResult> InvokeAsync(IEnumerable<string> selectedRoles, string htmlName, IEnumerable<string> except = null)
        {
            selectedRoles ??= Array.Empty<string>();

            var roleSelections = await BuildRoleSelectionsAsync(selectedRoles, except);

            var model = new SelectRolesViewModel
            {
                HtmlName = htmlName,
                RoleSelections = roleSelections
            };

            return View(model);
        }

        private async Task<IList<Selection<string>>> BuildRoleSelectionsAsync(IEnumerable<string> selectedRoles, IEnumerable<string> except)
        {
            var roleNames = await _roleService.GetRoleNamesAsync();

            if (except != null)
            {
                roleNames = roleNames.Except(except, StringComparer.OrdinalIgnoreCase);
            }

            return roleNames.Select(x => new Selection<string>
            {
                IsSelected = selectedRoles.Contains(x),
                Item = x
            })
            .OrderBy(x => x.Item)
            .ToList();
        }
    }

    public class SelectRolesViewModel
    {
        public string HtmlName { get; set; }
        public IList<Selection<string>> RoleSelections { get; set; }
    }

    public class Selection<T>
    {
        public bool IsSelected { get; set; }
        public T Item { get; set; }
    }
}
