using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.ViewModels;
using YesSql;

namespace OrchardCore.Taxonomies.Controllers
{
    [Admin]
    public class TagController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;

        public TagController(
            ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IContentDefinitionManager contentDefinitionManager)
        {
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(string taxonomyContentItemId, string displayText)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                return Unauthorized();
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

            var part = taxonomy.As<TaxonomyPart>();

            // Create tag term without running content item display manager update editor.
            // This creates empty parts, if parts are attached to the tag term, with no data.
            // This allows parts that have validation = required to still be created, and
            // later edited with the taxonomy editor.
            var contentItem = await _contentManager.NewAsync(part.TermContentType);
            contentItem.DisplayText = displayText;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Tag terms are always added to the root taxonomy element.
            taxonomy.Alter<TaxonomyPart>(part => part.Terms.Add(contentItem));

            // Auto publish draftable taxonomies when creating a new tag term.
            if (contentTypeDefinition.GetSettings<ContentTypeSettings>().Draftable)
            {
                await _contentManager.PublishAsync(taxonomy);
            }
            else
            {
                _session.Save(taxonomy);
            }

            var viewModel = new CreatedTagViewModel
            {
                ContentItemId = contentItem.ContentItemId,
                DisplayText = contentItem.DisplayText
            };

            return Ok(viewModel);
        }
    }
}

