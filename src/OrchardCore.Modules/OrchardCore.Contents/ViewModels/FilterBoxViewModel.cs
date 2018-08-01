using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Contents.ViewModels
{
    public class FilterBoxViewModel
    {
        public FilterBoxViewModel()
        {
            Options = new ContentOptions();
        }

        public ContentOptions Options { get; set; }

        public List<SelectListItem> ContentStatuses { get; set; }
        public List<SelectListItem> ContentSorts { get; set; }
        public List<SelectListItem> ContentTypes { get; set; }
        public List<SelectListItem> SortDirections { get; set; }
    }


    public class ContentOptions
    {
        public ContentOptions()
        {
            OrderBy = ContentsOrder.Modified;        
            ContentsStatus = ContentsStatus.AllVersions;

            //BulkAction = ContentsBulkAction.None;
        }
        public string TypeName { get; set; }
        public string TypeDisplayName { get; set; }
        public bool OwnedByMe { get; set; }
        public ContentsOrder OrderBy { get; set; }
        public SortDirection SortDirection { get; set; }
        public ContentsStatus ContentsStatus { get; set; }

        //public string SelectedCulture { get; set; }
        //public ContentsBulkAction BulkAction { get; set; }
        //public IEnumerable<string> Cultures { get; set; }
    }

    public enum ContentsOrder
    {
        Modified,
        Published,
        Created
    }


    public enum ContentsStatus
    {
        Draft,
        Published,
        AllVersions
    }

    public enum SortDirection
    {
        Descending,
        Ascending
    }
    //public enum ContentsBulkAction
    //{
    //    None,
    //    PublishNow,
    //    Unpublish,
    //    Remove
    //}
}
