using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Contents.ViewComponents
{
    public class DisplayContentItemViewComponent : ViewComponent
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IUpdateModelAccessor _modelUpdaterAccessor;

        public DisplayContentItemViewComponent(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IUpdateModelAccessor modelUpdaterAccessor)
        {
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
            _modelUpdaterAccessor = modelUpdaterAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync(string contentItemId = null, string displayType = null)
        {
            ContentItem contentItem = null;

            if (contentItemId != null)
            {
                contentItem = await _contentManager.GetAsync(contentItemId);
            }

            if (contentItem == null)
            {
                throw new ArgumentException("Content item not found");
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _modelUpdaterAccessor.ModelUpdater, displayType);

            return View(model);
        }
    }
}
