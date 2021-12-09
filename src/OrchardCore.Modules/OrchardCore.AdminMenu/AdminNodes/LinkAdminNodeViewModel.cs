using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class LinkAdminNodeViewModel
    {
        [Required]
        public string LinkText { get; set; }

        [Required]
        public string LinkUrl { get; set; }

        public string IconClass { get; set; }

        public string SelectedPermissionNames { get; set; }

        [BindNever]
        public IList<PermissionViewModel> SelectedItems { get; set; }

        [BindNever]
        public IList<PermissionViewModel> AllItems { get; set; }
    }
}
