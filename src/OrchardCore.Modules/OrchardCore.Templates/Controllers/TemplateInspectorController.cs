using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Modules;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
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
[Feature("OrchardCore.Templates.Assisted")]
[Admin("TemplateInspector/{action}", "TemplateInspector.{action}")]
public sealed class TemplateInspectorController : Controller
{
    private const int RecentContentItemsCount = 50;

    private readonly IAuthorizationService _authorizationService;
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ISession _session;
    private readonly TemplateInspectionService _templateInspectionService;
    private readonly SourceTemplatesService _sourceTemplatesService;
    private readonly TemplatesManager _templatesManager;
    private readonly AdminTemplatesManager _adminTemplatesManager;

    internal readonly IStringLocalizer S;

    public TemplateInspectorController(
        IAuthorizationService authorizationService,
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        ISession session,
        TemplateInspectionService templateInspectionService,
        SourceTemplatesService sourceTemplatesService,
        TemplatesManager templatesManager,
        AdminTemplatesManager adminTemplatesManager,
        IStringLocalizer<TemplateInspectorController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
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
            ContentItems = await GetRecentContentItemsAsync(contentItemId),
        };

        if (!string.IsNullOrEmpty(contentItemId))
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Published)
                ?? await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
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

    // The latest content items, the most recently modified first, grouped by content type in the picker.
    private async Task<IReadOnlyList<SelectListItem>> GetRecentContentItemsAsync(string selectedId)
    {
        var contentItems = await _session.Query<ContentItem, ContentItemIndex>(index => index.Latest)
            .OrderByDescending(index => index.ModifiedUtc)
            .Take(RecentContentItemsCount)
            .ListAsync();

        var groups = new Dictionary<string, SelectListGroup>(StringComparer.OrdinalIgnoreCase);
        var items = new List<SelectListItem>();

        foreach (var contentItem in contentItems.OrderBy(item => item.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            if (!groups.TryGetValue(contentItem.ContentType, out var group))
            {
                var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);
                group = groups[contentItem.ContentType] = new SelectListGroup { Name = typeDefinition?.DisplayName ?? contentItem.ContentType };
            }

            items.Add(new SelectListItem(
                string.IsNullOrWhiteSpace(contentItem.DisplayText) ? contentItem.ContentItemId : contentItem.DisplayText,
                contentItem.ContentItemId,
                contentItem.ContentItemId == selectedId)
            {
                Group = group,
            });
        }

        return items;
    }

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
