using System.Collections.Generic;

namespace OrchardCore.ContentTypes.ViewModels
{
    public class AddPartsViewModel
    {
        public AddPartsViewModel()
        {
            PartSelections = new List<PartSelectionViewModel>();
        }

        public EditTypeViewModel Type { get; set; }
        public List<PartSelectionViewModel> PartSelections { get; set; }
    }

    public class AddReusablePartViewModel
    {
        public AddReusablePartViewModel()
        {
            PartSelections = new List<PartSelectionViewModel>();
        }

        public EditTypeViewModel Type { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<PartSelectionViewModel> PartSelections { get; set; }
        public string SelectedPartName { get; set; }
    }

    public class PartSelectionViewModel
    {
        public string PartName { get; set; }
        public string PartDisplayName { get; set; }
        public string PartDescription { get; set; }
        public bool IsSelected { get; set; }
    }
}
