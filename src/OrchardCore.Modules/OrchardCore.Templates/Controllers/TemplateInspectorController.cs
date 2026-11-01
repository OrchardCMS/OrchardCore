using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Modules;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;
using OrchardCore.Templates.ViewModels;
using YesSql;

namespace OrchardCore.Templates.Controllers;

/// <summary>
/// Inspects a content item: lists the shapes it renders, their alternates and the templates that render them.
/// </summary>
[Feature("OrchardCore.Templates.ShapeTracing")]
[Admin("TemplateInspector/{action}", "TemplateInspector.{action}")]
public sealed class TemplateInspectorController : Controller
{
    private const int RecentContentItemsCount = 50;

    // The pages of content items checked to find those the user can view, at most 500 content items.
    private const int RecentContentItemsMaxPages = 10;

    private readonly IAuthorizationService _authorizationService;
    private readonly IContentManager _contentManager;
    private readonly ISession _session;
    private readonly TemplateInspectionService _templateInspectionService;
    private readonly SourceTemplatesService _sourceTemplatesService;
    private readonly TemplatesManager _templatesManager;
    private readonly AdminTemplatesManager _adminTemplatesManager;

    internal readonly IStringLocalizer S;

    public TemplateInspectorController(
        IAuthorizationService authorizationService,
        IContentManager contentManager,
        ISession session,
        TemplateInspectionService templateInspectionService,
        SourceTemplatesService sourceTemplatesService,
        TemplatesManager templatesManager,
        AdminTemplatesManager adminTemplatesManager,
        IStringLocalizer<TemplateInspectorController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _contentManager = contentManager;
        _session = session;
        _templateInspectionService = templateInspectionService;
        _sourceTemplatesService = sourceTemplatesService;
        _templatesManager = templatesManager;
        _adminTemplatesManager = adminTemplatesManager;
        S = stringLocalizer;
    }

    [Admin("TemplateInspector", "TemplateInspector.Index")]
    public async Task<IActionResult> Index(string contentItemId = null, string displayType = null, bool adminTemplates = false)
    {
        if (!await IsAuthorizedAsync(adminTemplates))
        {
            return Forbid();
        }

        string[] displayTypes = adminTemplates
            ? [OrchardCoreConstants.DisplayType.SummaryAdmin, OrchardCoreConstants.DisplayType.DetailAdmin]
            : [OrchardCoreConstants.DisplayType.Detail, OrchardCoreConstants.DisplayType.Summary];

        if (!displayTypes.Contains(displayType))
        {
            displayType = displayTypes[0];
        }

        var model = new TemplateInspectorViewModel
        {
            AdminTemplates = adminTemplates,
            ContentItemId = contentItemId,
            DisplayType = displayType,
            DisplayTypes = displayTypes.Select(type => new SelectListItem(type, type, type == displayType)).ToList(),
        };

        if (!string.IsNullOrEmpty(contentItemId))
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Published)
                ?? await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await CanInspectAsync(contentItem))
            {
                return Forbid();
            }

            var localTemplates = adminTemplates
                ? await _adminTemplatesManager.GetTemplatesDocumentAsync()
                : await _templatesManager.GetTemplatesDocumentAsync();

            (model.Shapes, model.Error) = await _templateInspectionService.InspectAsync(contentItem, displayType, adminTemplates);
            model.ContentItem = contentItem;
            model.LocalTemplates = new HashSet<string>(localTemplates.Templates.Keys, StringComparer.OrdinalIgnoreCase);
            model.SourceTemplates = await GetSourceTemplatesAsync(model.Shapes, adminTemplates);
        }

        return View(model);
    }

    // The templates of the sources, e.g. of the active theme, bound to the types or the alternates of the shapes,
    // to view or clone them. The sources are in their registration order, the theme before the features, as the
    // template of the theme wins when both have one.
    private async Task<IReadOnlyDictionary<string, InspectedSourceTemplate>> GetSourceTemplatesAsync(IEnumerable<InspectedShape> shapes, bool adminTemplates)
    {
        var names = new HashSet<string>(shapes.SelectMany(shape => shape.Alternates.Append(shape.Type)), StringComparer.OrdinalIgnoreCase);
        var sourceTemplates = new Dictionary<string, InspectedSourceTemplate>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in _sourceTemplatesService.Sources)
        {
            foreach (var template in await source.GetTemplatesAsync(adminTemplates, null))
            {
                if (names.Contains(template.Name))
                {
                    sourceTemplates.TryAdd(template.Name, new InspectedSourceTemplate { Source = source.Name, Template = template });
                }
            }
        }

        return sourceTemplates;
    }

    /// <summary>
    /// Searches the content items to inspect, for the picker of the content items: the latest ones the user can inspect,
    /// those whose display text contains the query if any.
    /// </summary>
    public async Task<IActionResult> SearchContentItems(string query = null, bool adminTemplates = false)
    {
        if (!await IsAuthorizedAsync(adminTemplates))
        {
            return Forbid();
        }

        var contentItems = await GetInspectableContentItemsAsync(query);

        return Ok(contentItems.Select(ToPickerItem));
    }

    // The item of the content picker for a content item, which isn't a link to its editor.
    internal static object ToPickerItem(ContentItem contentItem) => new
    {
        id = contentItem.ContentItemId,
        displayText = string.IsNullOrWhiteSpace(contentItem.DisplayText) ? contentItem.ContentItemId : contentItem.DisplayText,
        hasPublished = contentItem.Published,
        isClickable = false,
    };

    // The latest content items the user can inspect, the most recently modified first, those whose display text contains
    // the query if any. A content item is inspected in its published version when it has one, so it's listed with it.
    private async Task<IReadOnlyList<ContentItem>> GetInspectableContentItemsAsync(string query)
    {
        return await ViewableContentItems.TakeAsync(
            async (page, pageSize) =>
            {
                var latestQuery = string.IsNullOrWhiteSpace(query)
                    ? _session.Query<ContentItem, ContentItemIndex>(index => index.Latest)
                    : _session.Query<ContentItem, ContentItemIndex>(index => index.Latest && index.DisplayText.Contains(query));

                var latest = (await latestQuery
                    .OrderByDescending(index => index.ModifiedUtc)
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .ListAsync())
                    .ToList();

                // The drafts of published content items are replaced by their published versions.
                var draftIds = latest.Where(contentItem => !contentItem.Published).Select(contentItem => contentItem.ContentItemId).ToArray();
                var published = draftIds.Length == 0
                    ? []
                    : (await _contentManager.GetAsync(draftIds, VersionOptions.Published)).ToDictionary(contentItem => contentItem.ContentItemId);

                return latest.Select(contentItem => published.GetValueOrDefault(contentItem.ContentItemId) ?? contentItem);
            },
            CanInspectAsync,
            RecentContentItemsCount,
            RecentContentItemsMaxPages);
    }

    // A content item can be inspected by the users who can view it, and, when it is a draft, preview it.
    private async Task<bool> CanInspectAsync(ContentItem contentItem)
        => await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem)
            && (contentItem.Published || await _authorizationService.AuthorizeAsync(User, CommonPermissions.PreviewContent, contentItem));

    private async Task<bool> IsAuthorizedAsync(bool adminTemplates)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Liquid.Permissions.ManageLiquidTemplates))
        {
            return false;
        }

        var permission = adminTemplates
            ? AdminTemplatesPermissions.ManageAdminTemplates
            : Permissions.ManageTemplates;

        return await _authorizationService.AuthorizeAsync(User, permission);
    }
}
