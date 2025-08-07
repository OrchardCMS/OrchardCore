using Microsoft.AspNetCore.Authorization;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using OrchardCore.Admin;
using System.Threading;
using OrchardCore.ContentManagement;
using System.Threading;
using OrchardCore.ContentManagement.Metadata;
using System.Threading;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Threading;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading;
using OrchardCore.Taxonomies.Models;
using System.Threading;
using OrchardCore.Taxonomies.ViewModels;
using System.Threading;
using YesSql;
using System.Threading;

namespace OrchardCore.Taxonomies.Controllers;

[Admin]
public sealed class TagController : Controller, IUpdateModel
{
    private readonly IContentManager _contentManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ISession _session;

    public TagController(
        IContentManager contentManager,
        IAuthorizationService authorizationService,
        IContentDefinitionManager contentDefinitionManager,
        ISession session)
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

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync("Taxonomy");
        var versionOption = VersionOptions.Latest;

        if (contentTypeDefinition.IsDraftable())
        {
            versionOption = VersionOptions.DraftRequired;
        }

        var taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, versionOption);

        if (taxonomy == null)
        {
            return NotFound();
        }

        var part = taxonomy.As<TaxonomyPart>();

        // Create tag term but only run content handlers not content item display manager update editor.
        // This creates empty parts, if parts are attached to the tag term, with empty data.
        // But still generates valid autoroute paths from the handler. 
        var contentItem = await _contentManager.NewAsync(part.TermContentType);
        contentItem.DisplayText = displayText;
        contentItem.Weld<TermPart>();
        contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

        var result = await _contentManager.ValidateAsync(contentItem);

        if (result.Succeeded)
        {
            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);
        }
        else
        {
            foreach (var error in result.Errors)
            {
                if (error.MemberNames != null && error.MemberNames.Any())
                {
                    foreach (var memberName in error.MemberNames)
                    {
                        ModelState.AddModelError(memberName, error.ErrorMessage);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            await _session.CancelAsync();

            return BadRequest();
        }

        // Tag terms are always added to the root taxonomy element.
        taxonomy.Alter<TaxonomyPart>(part => part.Terms.Add(contentItem));

        // Auto publish draftable taxonomies when creating a new tag term.
        if (contentTypeDefinition.IsDraftable())
        {
            await _contentManager.PublishAsync(taxonomy);
        }
        else
        {
            await _session.SaveAsync(taxonomy, cancellationToken: CancellationToken.None);
        }

        var viewModel = new CreatedTagViewModel
        {
            ContentItemId = contentItem.ContentItemId,
            DisplayText = contentItem.DisplayText,
        };

        return Ok(viewModel);
    }
}
