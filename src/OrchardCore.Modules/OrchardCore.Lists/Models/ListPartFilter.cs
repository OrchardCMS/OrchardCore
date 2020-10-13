using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Lists.Models
{
    class ListPartFilter
    {
        public string DisplayText { get; set; }

        public string SelectedContentType { get; set; }

        public bool CanCreateSelectedContentType { get; set; }

        public ContentsOrder OrderBy { get; set; }

        public ContentsStatus ContentsStatus { get; set; }

        public ContentsBulkAction BulkAction { get; set; }
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
