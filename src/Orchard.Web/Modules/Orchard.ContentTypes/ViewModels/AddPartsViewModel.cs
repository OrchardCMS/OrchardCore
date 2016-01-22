using System.Collections.Generic;

namespace Orchard.ContentTypes.ViewModels {
    public class AddPartsViewModel {
        public AddPartsViewModel() {
            PartSelections = new List<PartSelectionViewModel>();
        }

        public EditTypeViewModel Type { get; set; }
        public IEnumerable<PartSelectionViewModel> PartSelections { get; set; }
    }

    public class PartSelectionViewModel {
        public string PartName { get; set; }
        public string PartDisplayName { get; set; }
        public string PartDescription { get;set; }
        public bool IsSelected { get; set; }
    }
}
