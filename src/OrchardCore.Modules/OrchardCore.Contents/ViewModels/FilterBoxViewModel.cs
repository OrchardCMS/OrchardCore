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
    }


    public class ContentOptions
    {
        public ContentOptions()
        {
            OrderBy = ContentsOrder.Modified;        
            ContentsStatus = ContentsStatus.Latest;

            //BulkAction = ContentsBulkAction.None;
        }
        public string TypeName { get; set; }
        public string TypeDisplayName { get; set; }
        public ContentsOrder OrderBy { get; set; }
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
        AllVersions,
        Latest,
        Owner
    }

    //public enum ContentsBulkAction
    //{
    //    None,
    //    PublishNow,
    //    Unpublish,
    //    Remove
    //}
}
