using Microsoft.AspNetCore.Authorization;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using OrchardCore.Admin;
using System.Threading;
using OrchardCore.ContentFields.Settings;
using System.Threading;
using OrchardCore.ContentFields.ViewModels;
using System.Threading;
using OrchardCore.ContentLocalization;
using System.Threading;
using OrchardCore.ContentManagement;
using System.Threading;
using OrchardCore.ContentManagement.Metadata;
using System.Threading;
using OrchardCore.ContentManagement.Records;
using System.Threading;
using OrchardCore.Contents;
using System.Threading;
using OrchardCore.Modules;
using System.Threading;
using YesSql;
using System.Threading;
using YesSql.Services;
using System.Threading;
using IHttpContextAccessor = Microsoft.AspNetCore.Http.IHttpContextAccessor;
using System.Threading;

namespace OrchardCore.ContentFields.Controllers;

[RequireFeatures("OrchardCore.ContentLocalization")]
[Admin]
public sealed class LocalizationSetContentPickerAdminController : Controller
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentLocalizationManager _contentLocalizationManager;
    private readonly IContentManager _contentManager;
    private readonly ISession _session;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalizationSetContentPickerAdminController(
        IContentDefinitionManager contentDefinitionManager,
        IContentLocalizationManager contentLocalizationManager,
        IContentManager contentManager,
        ISession session,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentLocalizationManager = contentLocalizationManager;
        _contentManager = contentManager;
        _session = session;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    [Admin("ContentFields/SearchLocalizationSets", "SearchLocalizationSets")]
    public async Task<IActionResult> SearchLocalizationSets(string part, string field, string query)
    {
        if (string.IsNullOrWhiteSpace(part) || string.IsNullOrWhiteSpace(field))
        {
            return BadRequest("Part and field are required parameters");
        }

        var partFieldDefinition = (await _contentDefinitionManager.GetPartDefinitionAsync(part))?.Fields
            .FirstOrDefault(f => f.Name == field);

        var fieldSettings = partFieldDefinition?.GetSettings<LocalizationSetContentPickerFieldSettings>();
        if (fieldSettings == null)
        {
            return BadRequest("Unable to find field definition");
        }

        var dbQuery = _session.Query<ContentItem, ContentItemIndex>()
          .With<ContentItemIndex>(x => x.ContentType.IsIn(fieldSettings.DisplayedContentTypes) && x.Latest);

        if (!string.IsNullOrEmpty(query))
        {
            dbQuery.With<ContentItemIndex>(x => x.DisplayText.Contains(query) || x.ContentType.Contains(query));
        }

        var contentItems = await dbQuery.Take(40).ListAsync(cancellationToken: CancellationToken.None);

        // if 2 search results have the same set, select one based on the current culture
        var cleanedContentItems = await _contentLocalizationManager.DeduplicateContentItemsAsync(contentItems);

        var results = new List<VueMultiselectItemViewModel>();

        foreach (var contentItem in cleanedContentItems)
        {
            if (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.ViewContent, contentItem))
            {
                results.Add(new VueMultiselectItemViewModel
                {
                    Id = contentItem.Key, // localization set
                    DisplayText = contentItem.Value.ToString(),
                    HasPublished = await _contentManager.HasPublishedVersionAsync(contentItem.Value),
                });
            }
        }

        return new ObjectResult(results);
    }
}
