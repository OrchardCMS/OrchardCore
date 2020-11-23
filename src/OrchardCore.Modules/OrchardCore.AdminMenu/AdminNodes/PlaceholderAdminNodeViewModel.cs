using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.AdminMenu.ViewModels;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class PlaceholderAdminNodeViewModel
    {
        [Required]
        public string LinkText { get; set; }

        public string IconClass { get; set; }

        public string PermissionIds { get; set; }

        [BindNever]
        public IList<VueMultiselectItemViewModel> SelectedItems { get; set; }

        [BindNever]
        public IList<VueMultiselectItemViewModel> AllItems { get; set; }
    }
}
