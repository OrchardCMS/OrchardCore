using OrchardCore.Contents.Workflows.Activities;

namespace OrchardCore.Contents.Workflows.ViewModels
{
    public class RetrieveContentTaskViewModel : ContentTaskViewModel<RetrieveContentTask>
    {
        /// <summary>
        /// The expression resulting into a content item or content item ID to unpublish.
        /// </summary>
        public string ContentItemId { get; set; }
    }
}
