using System;

namespace OrchardCore.Contents.AdminNodes
{
    public class ContentTypesAdminNodeViewModel
    {
        public bool ShowAll { get; set; }
        public string IconClass { get; set; }
        public ContentTypeEntryViewModel[] ContentTypes { get; set; } = Array.Empty<ContentTypeEntryViewModel>();
    }

    public class ContentTypeEntryViewModel
    {
        public bool IsChecked { get; set; }
        public string ContentTypeId { get; set; }
        public string IconClass { get; set; }
    }
}
