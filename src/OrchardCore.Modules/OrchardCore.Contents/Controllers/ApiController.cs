using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using Orchard.DisplayManagement.ModelBinding;

namespace OrchardCore.Content.Controllers
{
    public class ApiController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;

        public ApiController(
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            ITypeActivatorFactory<ContentPart> contentPartFactory)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            _contentPartFactory = contentPartFactory;
        }

        public async Task<IActionResult> Get(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            {
                return Unauthorized();
            }

            return new ObjectResult(contentItem);
        }

        public async Task<IActionResult> GetByVersion(string contentItemId, VersionOptions versionOptions)
        {
            if (versionOptions.IsAllVersions)
            {
                throw new Exception("TODO");
            }

            var contentItem = await _contentManager.GetAsync(contentItemId, versionOptions);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            {
                return Unauthorized();
            }

            return new ObjectResult(contentItem);
        }
    }
}
