using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.AdminMenu.ViewModels
{
    public interface IPermissionPickerViewModel {

        string PermissionIds { get; set; }

        [BindNever]
        IList<VueMultiselectItemViewModel> SelectedItems { get; set; }

        [BindNever]
        IList<VueMultiselectItemViewModel> AllItems { get; set; }

    }
}