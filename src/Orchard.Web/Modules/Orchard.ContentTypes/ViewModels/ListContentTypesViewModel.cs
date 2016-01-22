using System.Collections.Generic;

namespace Orchard.ContentTypes.ViewModels {
    public class ListContentTypesViewModel  {
        public IEnumerable<EditTypeViewModel> Types { get; set; }
    }
}