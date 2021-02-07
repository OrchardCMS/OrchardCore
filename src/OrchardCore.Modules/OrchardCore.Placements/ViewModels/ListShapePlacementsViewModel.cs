using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Placements.ViewModels
{
    public class ListShapePlacementsViewModel
    {
        public IList<ShapePlacementViewModel> ShapePlacements { get; set; }
        public dynamic Pager { get; set; }
        public ContentOptions Options { get; set; } = new ContentOptions();
    }

    public class ContentOptions
    {
        public string Search { get; set; }
        public ContentsBulkAction BulkAction { get; set; }

        #region Lists to populate

        [BindNever]
        public List<SelectListItem> ContentsBulkAction { get; set; }

        #endregion Lists to populate
    }

    public enum ContentsBulkAction
    {
        None,
        Remove
    }
}
