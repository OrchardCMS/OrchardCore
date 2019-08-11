using OrchardCore.Contents.Workflows.Activities;

namespace OrchardCore.Contents.Workflows.ViewModels
{
    public class DeleteContentTaskViewModel : ContentTaskViewModel<DeleteContentTask>
    {
        /// <summary>
        /// The expression resulting into a content item or content item ID to delete.
        /// </summary>
        public string Expression { get; set; }
    }
}
