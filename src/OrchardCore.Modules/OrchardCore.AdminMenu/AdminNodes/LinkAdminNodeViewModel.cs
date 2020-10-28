using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.AdminMenu.ViewModels;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class LinkAdminNodeViewModel
    {
        [Required]
        public string LinkText { get; set; }

        [Required]
        public string LinkUrl { get; set; }

        public string IconClass { get; set; }

        public string PermissionIds { get; set; }

        [BindNever]
        public IList<VueMultiselectItemViewModel> SelectedItems { get; set; }
    }
}
