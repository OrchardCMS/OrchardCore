using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using System.Threading.Tasks;
using JsonApiSerializer;
using JsonApiSerializer.JsonApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using Orchard.ContentManagement.MetaData;
using System.Collections.Generic;
using Orchard.ContentManagement.Api;
using Orchard.DisplayManagement.ModelBinding;

namespace OrchardCore.Content.Controllers
{
    public class ApiController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IApiContentManager _apiContentManager;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;

        public ApiController(
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IContentDefinitionManager contentDefinitionManager,
            IApiContentManager apiContentManager,
            ITypeActivatorFactory<ContentPart> contentPartFactory)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _apiContentManager = apiContentManager;
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

            var item = await _apiContentManager.BuildAsync(contentItem, Url, this);

            return Json(item, new JsonApiSerializerSettings());
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

            var item = await _apiContentManager.BuildAsync(contentItem, Url, this);

            return Json(item, new JsonApiSerializerSettings());
        }
    }
}
