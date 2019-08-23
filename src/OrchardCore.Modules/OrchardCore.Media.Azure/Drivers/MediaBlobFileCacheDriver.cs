using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.FileStorage;
using OrchardCore.Media.Azure.Models;

namespace OrchardCore.Media.Azure.Drivers
{
    public class MediaBlobFileCacheDriver : DisplayDriver<FileCache, MediaBlobFileCache>
    {
        public override IDisplayResult Display(MediaBlobFileCache fileCache)
        {
            return View("MediaBlobFileCache_SummaryAdmin", fileCache).Location("SummaryAdmin", "Content");
            //return Combine(
            //    View("LinkAdminNode_Fields_TreeSummary", fileCache).Location("TreeSummary", "Content"),
            //    View("LinkAdminNode_Fields_TreeThumbnail", fileCache).Location("TreeThumbnail", "Content")
            //);
        }
    }
}
