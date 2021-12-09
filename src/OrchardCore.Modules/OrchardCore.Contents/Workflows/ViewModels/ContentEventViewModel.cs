using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.ViewModels
{
    public class ContentEventViewModel<T> : ActivityViewModel<T> where T : ContentEvent
    {
        public ContentEventViewModel()
        {
        }

        public ContentEventViewModel(T activity)
        {
            Activity = activity;
        }

        public IList<ContentTypeDefinition> ContentTypeFilter { get; set; }
        public IList<string> SelectedContentTypeNames { get; set; } = new List<string>();
    }
}
