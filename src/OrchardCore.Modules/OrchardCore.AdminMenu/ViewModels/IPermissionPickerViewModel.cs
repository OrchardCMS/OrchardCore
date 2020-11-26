using System.Collections.Generic;

namespace OrchardCore.AdminMenu.ViewModels
{
    public interface IPermissionPickerViewModel {

        string PermissionIds { get; set; }

        IList<VueMultiselectItemViewModel> SelectedItems { get; set; }

        IList<VueMultiselectItemViewModel> AllItems { get; set; }

    }
}