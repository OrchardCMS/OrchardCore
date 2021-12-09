using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Handlers
{
    /// <summary>
    /// Content Handler to delete files used on attached media fields once the content item is deleted.
    /// </summary>
    public class AttachedMediaFieldContentHandler : ContentHandlerBase
    {
        private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;

        public AttachedMediaFieldContentHandler(AttachedMediaFieldFileService attachedMediaFieldFileService)
        {
            _attachedMediaFieldFileService = attachedMediaFieldFileService;
        }

        public override async Task RemovedAsync(RemoveContentContext context)
        {
            if (context.NoActiveVersionLeft)
            {
                await _attachedMediaFieldFileService.DeleteContentItemFolderAsync(context.ContentItem);
            }
        }
    }
}
