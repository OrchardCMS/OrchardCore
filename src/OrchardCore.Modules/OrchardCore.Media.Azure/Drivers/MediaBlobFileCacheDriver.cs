using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.Azure.Models;
using OrchardCore.Media.Azure.ViewModel;

namespace OrchardCore.Media.Azure.Drivers
{
    public class MediaBlobFileCacheDriver : DisplayDriver<MediaFileCache, MediaBlobFileCache>
    {
        public override IDisplayResult Display(MediaBlobFileCache fileCache)
        {
            return Initialize<MediaBlobFileCacheViewModel>("MediaBlobFileCache_SummaryAdmin", m => BuildViewModelAsync(m, fileCache))
                .Location("SummaryAdmin", "Content");
        }

        private Task BuildViewModelAsync(MediaBlobFileCacheViewModel m, MediaBlobFileCache fileCache)
        {
            m.Name = fileCache.Name;
            return Task.CompletedTask;
        }
    }
}
