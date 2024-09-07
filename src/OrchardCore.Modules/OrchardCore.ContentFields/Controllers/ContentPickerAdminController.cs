using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;

namespace OrchardCore.ContentFields.Controllers;

[Admin]
public sealed class ContentPickerAdminController : Controller
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IContentPickerResultProvider> _resultProviders;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContentPickerAdminController(
        IContentDefinitionManager contentDefinitionManager,
        IContentManager contentManager,
        IEnumerable<IContentPickerResultProvider> resultProviders,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentManager = contentManager;
        _resultProviders = resultProviders;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    [Admin("ContentFields/SearchContentItems", "ContentPicker")]
    public async Task<IActionResult> SearchContentItems(string part, string field, string query)
    {
        if (string.IsNullOrWhiteSpace(part) || string.IsNullOrWhiteSpace(field))
        {
            return BadRequest("Part and field are required parameters");
        }

        var partFieldDefinition = (await _contentDefinitionManager.GetPartDefinitionAsync(part))?.Fields
            .FirstOrDefault(f => f.Name == field);

        var fieldSettings = partFieldDefinition?.GetSettings<ContentPickerFieldSettings>();
        if (fieldSettings == null)
        {
            return BadRequest("Unable to find field definition");
        }

        var editor = partFieldDefinition.Editor() ?? "Default";

        var resultProvider = _resultProviders.FirstOrDefault(p => p.Name == editor)
            ?? _resultProviders.FirstOrDefault(p => p.Name == "Default");

        if (resultProvider == null)
        {
            return new ObjectResult(new List<ContentPickerResult>());
        }

        var contentTypes = fieldSettings.DisplayedContentTypes;

        if (fieldSettings.DisplayedStereotypes != null && fieldSettings.DisplayedStereotypes.Length > 0)
        {
            contentTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
                .Where(contentType =>
                {
                    var hasStereotype = contentType.TryGetStereotype(out var stereotype);

                    return hasStereotype && fieldSettings.DisplayedStereotypes.Contains(stereotype);
                }).Select(contentType => contentType.Name)
                .ToArray();
        }

        var results = await resultProvider.Search(new ContentPickerSearchContext
        {
            Query = query,
            DisplayAllContentTypes = fieldSettings.DisplayAllContentTypes,
            ContentTypes = contentTypes,
            PartFieldDefinition = partFieldDefinition,
        });

        var contentItems = await _contentManager
            .GetAsync(results.Select(r => r.ContentItemId));

        var selectedItems = new List<VueMultiselectItemViewModel>();
        var user = _httpContextAccessor.HttpContext?.User;
        foreach (var contentItem in contentItems)
        {
            selectedItems.Add(new VueMultiselectItemViewModel()
            {
                Id = contentItem.ContentItemId,
                DisplayText = contentItem.ToString(),
                HasPublished = contentItem.IsPublished(),
                IsViewable = await _authorizationService.AuthorizeAsync(user, CommonPermissions.EditContent, contentItem)
            });
        }

        return new ObjectResult(selectedItems);
    }
}
