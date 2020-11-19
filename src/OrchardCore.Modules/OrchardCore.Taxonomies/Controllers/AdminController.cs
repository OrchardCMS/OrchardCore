using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.Services;
using YesSql;

namespace OrchardCore.Taxonomies.Controllers
{
    public class AdminController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly IHtmlLocalizer H;
        private readonly INotifier _notifier;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly ITaxonomyFieldService _taxonomyFieldService;

        public AdminController(
            ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            IHtmlLocalizer<AdminController> localizer,
            IUpdateModelAccessor updateModelAccessor,
            ITaxonomyFieldService taxonomyFieldService)
        {
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            _notifier = notifier;
            _updateModelAccessor = updateModelAccessor;
            _taxonomyFieldService = taxonomyFieldService;
            H = localizer;
        }

        public async Task<IActionResult> Create(string id, string taxonomyContentItemId, string taxonomyItemId)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                return Forbid();
            }

            var contentItem = await _contentManager.NewAsync(id);
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            model.TaxonomyContentItemId = taxonomyContentItemId;
            model.TaxonomyItemId = taxonomyItemId;

            return View(model);
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(string id, string taxonomyContentItemId, string taxonomyItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                return Forbid();
            }

            ContentItem taxonomy;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Taxonomy");

            if (!contentTypeDefinition.GetSettings<ContentTypeSettings>().Draftable)
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);
            }
            else
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.DraftRequired);
            }

            if (taxonomy == null)
            {
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(id);
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            if (!ModelState.IsValid)
            {
                model.TaxonomyContentItemId = taxonomyContentItemId;
                model.TaxonomyItemId = taxonomyItemId;

                return View(model);
            }

            if (taxonomyItemId == null)
            {
                // Use the taxonomy as the parent if no target is specified
                taxonomy.Alter<TaxonomyPart>(part => part.Terms.Add(contentItem));
            }
            else
            {
                // Look for the target taxonomy item in the hierarchy
                var parentTaxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

                // Couldn't find targeted taxonomy item
                if (parentTaxonomyItem == null)
                {
                    return NotFound();
                }

                var taxonomyItems = parentTaxonomyItem?.Terms as JArray;

                if (taxonomyItems == null)
                {
                    parentTaxonomyItem["Terms"] = taxonomyItems = new JArray();
                }

                taxonomyItems.Add(JObject.FromObject(contentItem));
            }

            _session.Save(taxonomy);

            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = taxonomyContentItemId });
        }

        public async Task<IActionResult> Edit(string taxonomyContentItemId, string taxonomyItemId)
        {
            var taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);

            if (taxonomy == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies, taxonomy))
            {
                return Forbid();
            }

            // Look for the target taxonomy item in the hierarchy
            JObject taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            // Couldn't find targeted taxonomy item
            if (taxonomyItem == null)
            {
                return NotFound();
            }

            var contentItem = taxonomyItem.ToObject<ContentItem>();
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            model.TaxonomyContentItemId = taxonomyContentItemId;
            model.TaxonomyItemId = taxonomyItemId;

            return View(model);
        }

        [HttpPost]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(string taxonomyContentItemId, string taxonomyItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                return Forbid();
            }

            ContentItem taxonomy;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Taxonomy");

            if (!contentTypeDefinition.GetSettings<ContentTypeSettings>().Draftable)
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);
            }
            else
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.DraftRequired);
            }

            if (taxonomy == null)
            {
                return NotFound();
            }

            // Look for the target taxonomy item in the hierarchy
            JObject taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            // Couldn't find targeted taxonomy item
            if (taxonomyItem == null)
            {
                return NotFound();
            }

            var existing = taxonomyItem.ToObject<ContentItem>();

            // Create a new item to take into account the current type definition.
            var contentItem = await _contentManager.NewAsync(existing.ContentType);

            contentItem.Merge(existing);            
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            if (!ModelState.IsValid)
            {
                model.TaxonomyContentItemId = taxonomyContentItemId;
                model.TaxonomyItemId = taxonomyItemId;

                return View(model);
            }

            taxonomyItem.Merge(contentItem.Content, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });

            // Merge doesn't copy the properties
            taxonomyItem[nameof(ContentItem.DisplayText)] = contentItem.DisplayText;

            _session.Save(taxonomy);

            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = taxonomyContentItemId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string taxonomyContentItemId, string taxonomyItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                return Forbid();
            }

            ContentItem taxonomy;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Taxonomy");

            if (!contentTypeDefinition.GetSettings<ContentTypeSettings>().Draftable)
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);
            }
            else
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.DraftRequired);
            }

            if (taxonomy == null)
            {
                return NotFound();
            }

            // Look for the target taxonomy item in the hierarchy
            var taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            // Couldn't find targeted taxonomy item
            if (taxonomyItem == null)
            {
                return NotFound();
            }

            taxonomyItem.Remove();
            _session.Save(taxonomy);

            _notifier.Success(H["Taxonomy item deleted successfully"]);

            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = taxonomyContentItemId });
        }

        public async Task<IActionResult> OrderCategorizedContentItems(string taxonomyContentItemId, string taxonomyItemId)
        {
            var taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);

            if (taxonomy == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies, taxonomy))
            {
                return Forbid();
            }

            var taxonomyPart = taxonomy.As<TaxonomyPart>();

            if (!taxonomyPart.EnableOrdering)
            {
                return NotFound();
            }

            JObject taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            if (taxonomyItem == null)
            {
                return NotFound();
            }

            var contentItem = taxonomyItem.ToObject<ContentItem>();
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);
            contentItem.Alter<TermPart>(t => t.OrderingPageSize = taxonomyPart.OrderingPageSize);
            contentItem.Alter<TermPart>(t => t.Ordering = true);

            dynamic model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater);

            model.Updater = _updateModelAccessor.ModelUpdater;

            // Is this the best way to get the TermPartViewModel created by the TermPart driver, to emulate in OrderCategorizedContentItems.cshtm the "@model TermPartViewModel" of a "normal" View like TermPart.cshtml?
            model.TermPartViewModel = (ViewModels.TermPartViewModel)((System.Collections.Generic.List<DisplayManagement.IPositioned>)((dynamic)model).Content.Items).Where(i => ((dynamic)i).Metadata.Type == "TermPart").First();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> OrderCategorizedContentItemsPost(string taxonomyContentItemId, string taxonomyItemId, int oldIndex, int newIndex, PagerSlimParameters pagerSlimParameters, int pageSize)
        {
            var taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);

            if (taxonomy == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies, taxonomy))
            {
                return Forbid();
            }

            JObject taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            if (taxonomyItem == null)
            {
                return NotFound();
            }

            var contentItem = taxonomyItem.ToObject<ContentItem>();

            contentItem.Weld<TermPart>();

            var termPart = contentItem.As<TermPart>();
            contentItem.Alter<TermPart>(t => t.Ordering = true);

            var pager = new PagerSlim(pagerSlimParameters, pageSize);

            var categorizedContentItems = (await _taxonomyFieldService.QueryCategorizedItemsAsync(termPart, true, pager)).ToList();

            var topIndex = Math.Min(newIndex, oldIndex);

            // highest (first) order value on the list of items that need to be updated
            var topOrderValue = _taxonomyFieldService.GetTaxonomyTermOrder(categorizedContentItems[topIndex], taxonomyItemId);

            // move the element
            var categorizedContentItem = categorizedContentItems[oldIndex];
            categorizedContentItems.Remove(categorizedContentItem);
            categorizedContentItems.Insert(newIndex, categorizedContentItem);

            // restrict the list to the elements whose order value needs to be updated
            categorizedContentItems = categorizedContentItems.GetRange(topIndex, Math.Abs(newIndex - oldIndex) + 1);

            // apply and save the new order values
            _taxonomyFieldService.SaveCategorizedItemsOrder(categorizedContentItems, taxonomyItemId, topOrderValue);

            return Ok();
        }

        private JObject FindTaxonomyItem(JObject contentItem, string taxonomyItemId)
        {
            if (contentItem["ContentItemId"]?.Value<string>() == taxonomyItemId)
            {
                return contentItem;
            }

            if (contentItem.GetValue("Terms") == null)
            {
                return null;
            }

            var taxonomyItems = (JArray)contentItem["Terms"];

            JObject result;

            foreach (JObject taxonomyItem in taxonomyItems)
            {
                // Search in inner taxonomy items
                result = FindTaxonomyItem(taxonomyItem, taxonomyItemId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
