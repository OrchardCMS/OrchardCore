using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.ViewModels
{
    public class ContentTaskViewModel<T> : ActivityViewModel<T> where T : ContentTask
    {
        public ContentTaskViewModel()
        {
        }

        public ContentTaskViewModel(T activity)
        {
            Activity = activity;
        }
    }
}
