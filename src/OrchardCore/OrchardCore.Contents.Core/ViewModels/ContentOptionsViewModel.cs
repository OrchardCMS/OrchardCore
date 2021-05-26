using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Services;
using YesSql.Filters.Query;

namespace OrchardCore.Contents.ViewModels
{
    public class ContentOptionsViewModel
    {
        public ContentOptionsViewModel()
        {
            OrderBy = ContentsOrder.Modified;
            BulkAction = ViewModels.ContentsBulkAction.None;
            ContentsStatus = ContentsStatus.Latest;
        }
        public string SearchText { get; set; }
        public string OriginalSearchText { get; set; }

        public string SelectedContentType { get; set; }

        public bool CanCreateSelectedContentType { get; set; }

        public ContentsOrder OrderBy { get; set; }

        public ContentsStatus ContentsStatus { get; set; }

        public ContentsBulkAction BulkAction { get; set; }

        [ModelBinder(BinderType = typeof(ContentItemFilterEngineModelBinder), Name = nameof(SearchText))]
        public QueryFilterResult<ContentItem> FilterResult { get; set; }

        #region Values to populate

        [BindNever]
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        [BindNever]
        public int ContentItemsCount { get; set; }

        [BindNever]
        public int TotalItemCount { get; set; }

        [BindNever]
        public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();

        #endregion

        #region Lists to populate

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
