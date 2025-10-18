using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Events;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.ContentTypes.Services;

public class ContentDefinitionViewModelService : IContentDefinitionViewModelService
{
    private readonly IEnumerable<Type> _contentPartTypes;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentDefinitionService _contentDefinitionService;
    private readonly IEnumerable<IContentDefinitionEventHandler> _contentDefinitionEventHandlers;
    protected readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public ContentDefinitionViewModelService(IContentDefinitionManager contentDefinitionManager
        , IEnumerable<Type> contentPartTypes
        , IEnumerable<IContentDefinitionEventHandler> contentDefinitionEventHandlers
        , IStringLocalizer s
        , ILogger<ContentDefinitionViewModelService> logger,
        IContentDefinitionService contentDefinitionService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentPartTypes = contentPartTypes;
        _contentDefinitionEventHandlers = contentDefinitionEventHandlers;
        S = s;
        _logger = logger;
        _contentDefinitionService = contentDefinitionService;
    }

    public async Task<EditPartViewModel> AddPartAsync(CreatePartViewModel partViewModel)
    {
        var partDefinition = await _contentDefinitionService.CreatePartDefinitionAsync(partViewModel.Name);

        if (partDefinition == null)
        {
            return null;
        }

        return new EditPartViewModel(partDefinition);
    }

    public async Task RemovePartAsync(string name)
    {
        await _contentDefinitionService.RemovePartDefinitionAsync(name);
    }

    public async Task AlterFieldAsync(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel)
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(partViewModel.Name, partBuilder =>
        {
            partBuilder.WithField(fieldViewModel.Name, fieldBuilder =>
            {
                fieldBuilder.WithDisplayName(fieldViewModel.DisplayName);
                fieldBuilder.WithEditor(fieldViewModel.Editor);
                fieldBuilder.WithDisplayMode(fieldViewModel.DisplayMode);
            });
        });

        var context = new ContentPartFieldUpdatedContext
        {
            ContentPartName = partViewModel.Name,
            ContentFieldName = fieldViewModel.Name,
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentPartFieldUpdated(ctx), context, _logger);
    }

    public async Task AlterTypePartAsync(EditTypePartViewModel typePartViewModel)
    {
        var typeDefinition = typePartViewModel.TypePartDefinition.ContentTypeDefinition;

        await _contentDefinitionManager.AlterTypeDefinitionAsync(typeDefinition.Name, type =>
        {
            type.WithPart(typePartViewModel.Name, typePartViewModel.TypePartDefinition.PartDefinition, part =>
            {
                part.WithDisplayName(typePartViewModel.DisplayName);
                part.WithDescription(typePartViewModel.Description);
                part.WithEditor(typePartViewModel.Editor);
                part.WithDisplayMode(typePartViewModel.DisplayMode);
            });
        });

        var context = new ContentTypePartUpdatedContext
        {
            ContentTypeName = typeDefinition.Name,
            ContentPartName = typePartViewModel.Name,
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentTypePartUpdated(ctx), context, _logger);
    }

    public async Task<EditPartViewModel> GetPartAsync(string name)
    {
        var contentPartDefinition = await _contentDefinitionManager.GetPartDefinitionAsync(name);

        if (contentPartDefinition == null)
        {
            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(name);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            contentPartDefinition = new ContentPartDefinition(name);
        }

        var viewModel = new EditPartViewModel(contentPartDefinition);

        return viewModel;
    }

    public async Task<IEnumerable<EditPartViewModel>> GetPartsAsync(bool metadataPartsOnly)
    {
        var typeNames = new HashSet<string>((await GetTypesAsync()).Select(ctd => ctd.Name));

        // User-defined parts.
        // Except for those parts with the same name as a type (implicit type's part or a mistake).
        var userContentParts = (await _contentDefinitionManager.ListPartDefinitionsAsync())
            .Where(cpd => !typeNames.Contains(cpd.Name))
            .Select(cpd => new EditPartViewModel(cpd))
            .ToDictionary(k => k.Name);

        // Code-defined parts.
        var codeDefinedParts = metadataPartsOnly
            ? []
            : _contentPartTypes
                .Where(cpd => !userContentParts.ContainsKey(cpd.Name))
                .Select(cpi => new EditPartViewModel
                {
                    Name = cpi.Name,
                    DisplayName = cpi.Name,
                }).ToList();

        // Order by display name.
        return codeDefinedParts
            .Union(userContentParts.Values)
            .OrderBy(m => m.DisplayName);
    }

    public async Task<EditTypeViewModel> GetTypeAsync(string name)
    {
        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(name);

        if (contentTypeDefinition == null)
        {
            return null;
        }

        return new EditTypeViewModel(contentTypeDefinition);
    }

    public async Task<IEnumerable<EditTypeViewModel>> GetTypesAsync()
        => (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Select(ctd => new EditTypeViewModel(ctd))
            .OrderBy(m => m.DisplayName);

    public async Task<EditPartViewModel> LoadPartAsync(string name)
    {
        var contentPartDefinition = await _contentDefinitionManager.LoadPartDefinitionAsync(name);

        if (contentPartDefinition == null)
        {
            var contentTypeDefinition = await _contentDefinitionManager.LoadTypeDefinitionAsync(name);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            contentPartDefinition = new ContentPartDefinition(name);
        }

        var viewModel = new EditPartViewModel(contentPartDefinition);

        return viewModel;
    }

    public async Task<IEnumerable<EditPartViewModel>> LoadPartsAsync(bool metadataPartsOnly)
    {
        var typeNames = new HashSet<string>((await LoadTypesAsync()).Select(ctd => ctd.Name));

        // User-defined parts.
        // Except for those parts with the same name as a type (implicit type's part or a mistake).
        var userContentParts = (await _contentDefinitionManager.LoadPartDefinitionsAsync())
            .Where(cpd => !typeNames.Contains(cpd.Name))
            .Select(cpd => new EditPartViewModel(cpd))
            .ToDictionary(k => k.Name);

        // Code-defined parts.
        var codeDefinedParts = metadataPartsOnly
            ? []
            : _contentPartTypes
                .Where(cpd => !userContentParts.ContainsKey(cpd.Name))
                .Select(cpi => new EditPartViewModel
                {
                    Name = cpi.Name,
                    DisplayName = cpi.Name,
                }).ToList();

        // Order by display name.
        return codeDefinedParts
            .Union(userContentParts.Values)
            .OrderBy(m => m.DisplayName);
    }

    public async Task<EditTypeViewModel> LoadTypeAsync(string name)
    {
        var contentTypeDefinition = await _contentDefinitionManager.LoadTypeDefinitionAsync(name);

        if (contentTypeDefinition == null)
        {
            return null;
        }

        return new EditTypeViewModel(contentTypeDefinition);
    }

    public async Task<IEnumerable<EditTypeViewModel>> LoadTypesAsync()
        => (await _contentDefinitionManager.LoadTypeDefinitionsAsync())
            .Select(ctd => new EditTypeViewModel(ctd))
            .OrderBy(m => m.DisplayName);
}
