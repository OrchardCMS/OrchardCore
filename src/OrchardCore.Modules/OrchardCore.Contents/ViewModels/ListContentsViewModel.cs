using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Contents.ViewModels
{
    public class ListContentsViewModel
    {
        public ListContentsViewModel()
        {
            Options = new ContentOptions();
        }

        public int? Page { get; set; }

        public ContentOptions Options { get; set; }

        [BindNever]
        public List<dynamic> ContentItems { get; set; }

        [BindNever]
        public dynamic Pager { get; set; }
    }

    public class ContentOptions
    {
        public ContentOptions()
        {
            OrderBy = ContentsOrder.Modified;
            BulkAction = ViewModels.ContentsBulkAction.None;
            ContentsStatus = ContentsStatus.Latest;
        }

        public string DisplayText { get; set; }

        public string SelectedContentType { get; set; }

        public bool CanCreateSelectedContentType { get; set; }

        public ContentsOrder OrderBy { get; set; }

        public ContentsStatus ContentsStatus { get; set; }

        public ContentsBulkAction BulkAction { get; set; }

        #region Lists to populate

        [BindNever]
        public List<SelectListItem> Cultures { get; set; }

        [BindNever]
        public List<SelectListItem> ContentStatuses { get; set; }

        [BindNever]
        public List<SelectListItem> ContentSorts { get; set; }

        [BindNever]
        public List<SelectListItem> ContentsBulkAction { get; set; }

        [BindNever]
        public List<SelectListItem> ContentTypeOptions { get; set; }

        [BindNever]
        public List<SelectListItem> CreatableTypes { get; set; }

        [BindNever]
        public List<SelectListItem> Users { get; set; }

        #endregion Lists to populate
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
