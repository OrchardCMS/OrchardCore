using OrchardCore.Contents.Workflows.Activities;

namespace OrchardCore.Contents.Workflows.ViewModels
{
    public class PublishContentTaskViewModel : ContentTaskViewModel<PublishContentTask>
    {
        /// <summary>
        /// The expression resulting into a content item or content item ID to publish.
        /// </summary>
        public string Expression { get; set; }
    }
}
