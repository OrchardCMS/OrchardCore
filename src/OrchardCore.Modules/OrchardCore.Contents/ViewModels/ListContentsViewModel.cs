using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.ViewModels
{
    public class ListContentsViewModel
    {
        public ListContentsViewModel()
        {
            Options = new ContentOptions();
        }

        public string ContentTypeName
        {
            get { return Options.SelectedContentTypeFilter; }
        }

        public string DisplayText { get; set; }
        public string ContentTypeDisplayName { get; set; }
        public int? Page { get; set; }

        public ContentOptions Options { get; set; }

        [BindNever]
        public List<dynamic> ContentItems { get; set; }

        public dynamic Pager { get; set; }

        public List<SelectListItem> ContentStatuses { get; set; }

        public List<SelectListItem> ContentSorts { get; set; }

        public List<SelectListItem> ContentsBulkAction { get; set; }

        public List<SelectListItem> ContentTypesFilterOptions { get; set; }

        //[BindNever]
        //public IList<Entry> Entries { get; set; }

        //#region Nested type: Entry

        //public class Entry
        //{
        //    public ContentItem ContentItem { get; set; }
        //    public ContentItemMetadata ContentItemMetadata { get; set; }
        //}

        //#endregion
    }

    public class ContentOptions
    {
        public ContentOptions()
        {
            OrderBy = ContentsOrder.Modified;
            BulkAction = ContentsBulkAction.None;
            ContentsStatus = ContentsStatus.Latest;
        }
        public string SelectedContentTypeFilter { get; set; }
        public string SelectedCulture { get; set; }
        public IEnumerable<KeyValuePair<string, string>> FilterOptions { get; set; }
        public ContentsOrder OrderBy { get; set; }
        public ContentsStatus ContentsStatus { get; set; }
        public ContentsBulkAction BulkAction { get; set; }
        public IEnumerable<string> Cultures { get; set; }
    }

    public enum ContentsOrder
    {
        Modified,
        Published,
        Created,
        Title,
    }

    public enum ContentsStatus
    {
        Draft,
        Published,
        AllVersions,
        Latest,
        Owner
    }

    public enum ContentsBulkAction
    {
        None,
        PublishNow,
        Unpublish,
        Remove
    }
}
