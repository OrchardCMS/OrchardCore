using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentManagement.Handlers
{
    public class UpdateContentContext : ContentContextBase
    {
        public UpdateContentContext(ContentItem contentItem, IUpdateModel updater) : base(contentItem)
        {
            UpdatingItem = contentItem;
            Updater = updater;
        }

        public ContentItem UpdatingItem { get; set; }
        public IUpdateModel Updater { get; }
    }
}