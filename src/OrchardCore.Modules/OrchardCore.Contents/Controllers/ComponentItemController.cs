using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Mvc.Utilities;
using OrchardCore.ContentManagement.Display.ViewModels;

namespace OrchardCore.Contents.Controllers
{
    public class ComponentItemController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IShapeFactory _shapeFactory;

        public ComponentItemController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IAuthorizationService authorizationService,
            IUpdateModelAccessor updateModelAccessor,
            IShapeFactory shapeFactory)
        {
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
            _updateModelAccessor = updateModelAccessor;
            _shapeFactory = shapeFactory;
        }

        public async Task<IActionResult> Display(string contentItemId, string jsonPath)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            // var componentModel = await _shapeFactory.CreateAsync<ContentItemComponentModel>("ContentItem", x =>
            // {
            //     x.ContentItem = contentItem;
            // });
            // This should be done by registering an event handler in the model.
            // componentModel.Metadata.Alternates.Add($"ContentItem__{contentItem.ContentType}");

            // This creates a dynamic shape.
            // var t= await _shapeFactory.CreateAsync("ContentItem", new ContentItemComponentModel(contentItem));

            // Technically we probably want to be able to produce these from DI. So it might need a registry.
            // Not totally sure. Depends how The component factory ends up looking.
            // This just a quick hack to give a specific model, directly to the shape factory, where the model knows what kind of shape it is.

            // so really we need a func to produce this as well.


            var componentModel = await _shapeFactory.CreateAsync(new ContentItemComponentModel(contentItem));

            return View(componentModel);
        }

        public async Task<IActionResult> Preview(string contentItemId)
        {
            if (contentItemId == null)
            {
                return NotFound();
            }

            var versionOptions = VersionOptions.Latest;

            var contentItem = await _contentManager.GetAsync(contentItemId, versionOptions);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.PreviewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater);

            return View(model);
        }
    }
}
