using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;

namespace OrchardCore.ContentPreview.Controllers
{
    public class PreviewController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IContentManagerSession _contentManagerSession;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IClock _clock;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public PreviewController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentManagerSession contentManagerSession,
            IAuthorizationService authorizationService,
            IClock clock,
            IUpdateModelAccessor updateModelAccessor)
        {
            _authorizationService = authorizationService;
            _clock = clock;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
            _contentManagerSession = contentManagerSession;
            _updateModelAccessor = updateModelAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Render()
        {
            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PreviewContent))
            {
                return this.ChallengeOrForbid();
            }

            // Mark request as a `Preview` request so that drivers / handlers or underlying services can be aware of an active preview mode.
            HttpContext.Features.Set(new ContentPreviewFeature());

            var contentItemType = Request.Form["ContentItemType"];
            var contentItem = await _contentManager.NewAsync(contentItemType);

            // Assign the ids from the currently edited item so that validation thinks
            // it's working on the same item. For instance if drivers are checking name unicity
            // they need to think this is the same existing item (AutoroutePart).

            var contentItemId = Request.Form["PreviewContentItemId"];
            var contentItemVersionId = Request.Form["PreviewContentItemVersionId"];

            // Unique contentItem.Id that only Preview is using such that another
            // stored document can't have the same one in the IContentManagerSession index.

            contentItem.Id = -1;
            contentItem.ContentItemId = contentItemId;
            contentItem.ContentItemVersionId = contentItemVersionId;
            contentItem.CreatedUtc = _clock.UtcNow;
            contentItem.ModifiedUtc = _clock.UtcNow;
            contentItem.PublishedUtc = _clock.UtcNow;
            contentItem.Published = true;

            // TODO: we should probably get this value from the main editor as it might impact validators.
            _ = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            if (!ModelState.IsValid)
            {
                var errors = new List<string>();
                foreach (var modelState in ValidationHelpers.GetModelStateList(ViewData, false))
                {
                    for (var i = 0; i < modelState.Errors.Count; i++)
                    {
                        var modelError = modelState.Errors[i];
                        var errorText = ValidationHelpers.GetModelErrorMessageOrDefault(modelError);
                        errors.Add(errorText);
                    }
                }

                return this.InternalServerError(new { errors });
            }

            var previewAspect = await _contentManager.PopulateAspectAsync(contentItem, new PreviewAspect());

            if (!String.IsNullOrEmpty(previewAspect.PreviewUrl))
            {
                // The PreviewPart is configured, we need to set the fake content item.
                _contentManagerSession.Store(contentItem);

                if (!previewAspect.PreviewUrl.StartsWith('/'))
                {
                    previewAspect.PreviewUrl = "/" + previewAspect.PreviewUrl;
                }

                Request.HttpContext.Items["PreviewPath"] = previewAspect.PreviewUrl;

                return Ok();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, "Detail");

            return View(model);
        }
    }

    internal static class ValidationHelpers
    {
        public static string GetModelErrorMessageOrDefault(ModelError modelError)
        {
            Debug.Assert(modelError != null);

            if (!String.IsNullOrEmpty(modelError.ErrorMessage))
            {
                return modelError.ErrorMessage;
            }

            // Default in the ValidationSummary case is no error message.
            return String.Empty;
        }

        public static string GetModelErrorMessageOrDefault(
            ModelError modelError,
            ModelStateEntry containingEntry,
            ModelExplorer modelExplorer)
        {
            Debug.Assert(modelError != null);
            Debug.Assert(containingEntry != null);
            Debug.Assert(modelExplorer != null);

            if (!String.IsNullOrEmpty(modelError.ErrorMessage))
            {
                return modelError.ErrorMessage;
            }

            // Default in the ValidationMessage case is a fallback error message.
            var attemptedValue = containingEntry.AttemptedValue ?? "null";
            return modelExplorer.Metadata.ModelBindingMessageProvider.ValueIsInvalidAccessor(attemptedValue);
        }

        // Returns non-null list of model states, which caller will render in order provided.
        public static IList<ModelStateEntry> GetModelStateList(
            ViewDataDictionary viewData,
            bool excludePropertyErrors)
        {
            if (excludePropertyErrors)
            {
                viewData.ModelState.TryGetValue(viewData.TemplateInfo.HtmlFieldPrefix, out var ms);

                if (ms != null)
                {
                    return new[] { ms };
                }
            }
            else if (viewData.ModelState.Count > 0)
            {
                var metadata = viewData.ModelMetadata;
                var modelStateDictionary = viewData.ModelState;
                var entries = new List<ModelStateEntry>();
                Visit(modelStateDictionary.Root, metadata, entries);

                if (entries.Count < modelStateDictionary.Count)
                {
                    // Account for entries in the ModelStateDictionary that do not have corresponding ModelMetadata values.
                    foreach (var entry in modelStateDictionary)
                    {
                        if (!entries.Contains(entry.Value))
                        {
                            entries.Add(entry.Value);
                        }
                    }
                }

                return entries;
            }

            return Array.Empty<ModelStateEntry>();
        }

        private static void Visit(
            ModelStateEntry modelStateEntry,
            ModelMetadata metadata,
            List<ModelStateEntry> orderedModelStateEntries)
        {
            if (metadata.ElementMetadata != null && modelStateEntry.Children != null)
            {
                foreach (var indexEntry in modelStateEntry.Children)
                {
                    Visit(indexEntry, metadata.ElementMetadata, orderedModelStateEntries);
                }
            }
            else
            {
                for (var i = 0; i < metadata.Properties.Count; i++)
                {
                    var propertyMetadata = metadata.Properties[i];
                    var propertyModelStateEntry = modelStateEntry.GetModelStateForProperty(propertyMetadata.PropertyName);
                    if (propertyModelStateEntry != null)
                    {
                        Visit(propertyModelStateEntry, propertyMetadata, orderedModelStateEntries);
                    }
                }
            }

            if (!modelStateEntry.IsContainerNode)
            {
                orderedModelStateEntries.Add(modelStateEntry);
            }
        }
    }
}
