using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;

namespace OrchardCore.ContentTypes.Editors;

public class DefaultContentDefinitionDisplayManager : BaseDisplayManager, IContentDefinitionDisplayManager
{
    private readonly IEnumerable<IContentDefinitionDisplayHandler> _handlers;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly ILogger _logger;

    public DefaultContentDefinitionDisplayManager(
        IEnumerable<IContentDefinitionDisplayHandler> handlers,
        IContentDefinitionManager contentDefinitionManager,
        IShapeFactory shapeFactory,
        IEnumerable<IShapePlacementProvider> placementProviders,
        ILogger<DefaultContentDefinitionDisplayManager> logger,
        ILayoutAccessor layoutAccessor
        ) : base(shapeFactory, placementProviders)
    {
        _handlers = handlers;
        _contentDefinitionManager = contentDefinitionManager;
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
        _logger = logger;
    }

    public async Task<dynamic> BuildTypeEditorAsync(ContentTypeDefinition contentTypeDefinition, IUpdateModel updater, string groupId)
    {
        ArgumentNullException.ThrowIfNull(contentTypeDefinition);

        var contentTypeDefinitionShape = await CreateContentShapeAsync("ContentTypeDefinition_Edit").ConfigureAwait(false);
        contentTypeDefinitionShape.Properties["ContentTypeDefinition"] = contentTypeDefinition;

        var typeContext = new BuildEditorContext(
            contentTypeDefinitionShape,
            groupId,
            false,
            "",
            _shapeFactory,
            await _layoutAccessor.GetLayoutAsync(),
            updater
        );

        await BindPlacementAsync(typeContext).ConfigureAwait(false);

        await _handlers.InvokeAsync((handler, contentTypeDefinition, typeContext) => handler.BuildTypeEditorAsync(contentTypeDefinition, typeContext), contentTypeDefinition, typeContext, _logger).ConfigureAwait(false);

        return contentTypeDefinitionShape;
    }

    public async Task<dynamic> UpdateTypeEditorAsync(ContentTypeDefinition contentTypeDefinition, IUpdateModel updater, string groupId)
    {
        ArgumentNullException.ThrowIfNull(contentTypeDefinition);

        var contentTypeDefinitionShape = await CreateContentShapeAsync("ContentTypeDefinition_Edit").ConfigureAwait(false);
        contentTypeDefinitionShape.Properties["ContentTypeDefinition"] = contentTypeDefinition;

        var layout = await _layoutAccessor.GetLayoutAsync().ConfigureAwait(false);

        await _contentDefinitionManager.AlterTypeDefinitionAsync(contentTypeDefinition.Name, async typeBuilder =>
        {
            var typeContext = new UpdateTypeEditorContext(
                typeBuilder,
                contentTypeDefinitionShape,
                groupId,
                false,
                _shapeFactory,
                layout,
                updater
            );

            await BindPlacementAsync(typeContext).ConfigureAwait(false);

            await _handlers.InvokeAsync((handler, contentTypeDefinition, typeContext) => handler.UpdateTypeEditorAsync(contentTypeDefinition, typeContext), contentTypeDefinition, typeContext, _logger).ConfigureAwait(false);
        }).ConfigureAwait(false);

        return contentTypeDefinitionShape;
    }

    public async Task<dynamic> BuildPartEditorAsync(ContentPartDefinition contentPartDefinition, IUpdateModel updater, string groupId)
    {
        ArgumentNullException.ThrowIfNull(contentPartDefinition);

        var contentPartDefinitionShape = await CreateContentShapeAsync("ContentPartDefinition_Edit").ConfigureAwait(false);

        var partContext = new BuildEditorContext(
            contentPartDefinitionShape,
            groupId,
            false,
            "",
            _shapeFactory,
            await _layoutAccessor.GetLayoutAsync(),
            updater
        );

        await BindPlacementAsync(partContext).ConfigureAwait(false);

        await _handlers.InvokeAsync((handler, contentPartDefinition, partContext) => handler.BuildPartEditorAsync(contentPartDefinition, partContext), contentPartDefinition, partContext, _logger).ConfigureAwait(false);

        return contentPartDefinitionShape;
    }

    public async Task<dynamic> UpdatePartEditorAsync(ContentPartDefinition contentPartDefinition, IUpdateModel updater, string groupId)
    {
        ArgumentNullException.ThrowIfNull(contentPartDefinition);

        var contentPartDefinitionShape = await CreateContentShapeAsync("ContentPartDefinition_Edit").ConfigureAwait(false);

        UpdatePartEditorContext partContext = null;
        var layout = await _layoutAccessor.GetLayoutAsync().ConfigureAwait(false);

        await _contentDefinitionManager.AlterPartDefinitionAsync(contentPartDefinition.Name, async partBuilder =>
        {
            partContext = new UpdatePartEditorContext(
                partBuilder,
                contentPartDefinitionShape,
                groupId,
                false,
                _shapeFactory,
                layout,
                updater
            );

            await BindPlacementAsync(partContext).ConfigureAwait(false);

            await _handlers.InvokeAsync((handler, contentPartDefinition, partContext) => handler.UpdatePartEditorAsync(contentPartDefinition, partContext), contentPartDefinition, partContext, _logger).ConfigureAwait(false);
        }).ConfigureAwait(false);

        return contentPartDefinitionShape;
    }

    public async Task<dynamic> BuildTypePartEditorAsync(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater, string groupId = "")
    {
        ArgumentNullException.ThrowIfNull(contentTypePartDefinition);

        var typePartDefinitionShape = await CreateContentShapeAsync("ContentTypePartDefinition_Edit").ConfigureAwait(false);
        typePartDefinitionShape.Properties["ContentPart"] = contentTypePartDefinition;

        var partContext = new BuildEditorContext(
            typePartDefinitionShape,
            groupId,
            false,
            "",
            _shapeFactory,
            await _layoutAccessor.GetLayoutAsync(),
            updater
        );

        await BindPlacementAsync(partContext).ConfigureAwait(false);

        await _handlers.InvokeAsync((handler, contentTypePartDefinition, partContext) => handler.BuildTypePartEditorAsync(contentTypePartDefinition, partContext), contentTypePartDefinition, partContext, _logger).ConfigureAwait(false);

        return typePartDefinitionShape;
    }

    public async Task<dynamic> UpdateTypePartEditorAsync(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater, string groupId = "")
    {
        ArgumentNullException.ThrowIfNull(contentTypePartDefinition);

        var typePartDefinitionShape = await CreateContentShapeAsync("ContentTypePartDefinition_Edit").ConfigureAwait(false);
        var layout = await _layoutAccessor.GetLayoutAsync().ConfigureAwait(false);

        await _contentDefinitionManager.AlterTypeDefinitionAsync(contentTypePartDefinition.ContentTypeDefinition.Name, typeBuilder =>
        {
            return typeBuilder.WithPartAsync(contentTypePartDefinition.Name, async typePartBuilder =>
            {
                typePartDefinitionShape.Properties["ContentPart"] = contentTypePartDefinition;

                var partContext = new UpdateTypePartEditorContext(
                    typePartBuilder,
                    typePartDefinitionShape,
                    groupId,
                    false,
                    _shapeFactory,
                    layout,
                    updater
                );

                await BindPlacementAsync(partContext).ConfigureAwait(false);

                await _handlers.InvokeAsync((handler, contentTypePartDefinition, partContext) => handler.UpdateTypePartEditorAsync(contentTypePartDefinition, partContext), contentTypePartDefinition, partContext, _logger).ConfigureAwait(false);
            });
        }).ConfigureAwait(false);

        return typePartDefinitionShape;
    }

    public async Task<dynamic> BuildPartFieldEditorAsync(ContentPartFieldDefinition contentPartFieldDefinition, IUpdateModel updater, string groupId = "")
    {
        ArgumentNullException.ThrowIfNull(contentPartFieldDefinition);

        var partFieldDefinitionShape = await CreateContentShapeAsync("ContentPartFieldDefinition_Edit").ConfigureAwait(false);
        partFieldDefinitionShape.Properties["ContentField"] = contentPartFieldDefinition;

        var fieldContext = new BuildEditorContext(
            partFieldDefinitionShape,
            groupId,
            false,
            "",
            _shapeFactory,
            await _layoutAccessor.GetLayoutAsync(),
            updater
        );

        await BindPlacementAsync(fieldContext).ConfigureAwait(false);

        await _handlers.InvokeAsync((handler, contentPartFieldDefinition, fieldContext) => handler.BuildPartFieldEditorAsync(contentPartFieldDefinition, fieldContext), contentPartFieldDefinition, fieldContext, _logger).ConfigureAwait(false);

        return partFieldDefinitionShape;
    }

    public async Task<dynamic> UpdatePartFieldEditorAsync(ContentPartFieldDefinition contentPartFieldDefinition, IUpdateModel updater, string groupId = "")
    {
        ArgumentNullException.ThrowIfNull(contentPartFieldDefinition);

        var contentPartDefinition = contentPartFieldDefinition.PartDefinition;
        var partFieldDefinitionShape = await CreateContentShapeAsync("ContentPartFieldDefinition_Edit").ConfigureAwait(false);

        var layout = await _layoutAccessor.GetLayoutAsync().ConfigureAwait(false);

        await _contentDefinitionManager.AlterPartDefinitionAsync(contentPartDefinition.Name, partBuilder =>
        {
            return partBuilder.WithFieldAsync(contentPartFieldDefinition.Name, async partFieldBuilder =>
            {
                partFieldDefinitionShape.Properties["ContentField"] = contentPartFieldDefinition;

                var fieldContext = new UpdatePartFieldEditorContext(
                    partFieldBuilder,
                    partFieldDefinitionShape,
                    groupId,
                    false,
                    _shapeFactory,
                    layout,
                    updater
                );

                await BindPlacementAsync(fieldContext).ConfigureAwait(false);

                await _handlers.InvokeAsync((handler, contentPartFieldDefinition, fieldContext) => handler.UpdatePartFieldEditorAsync(contentPartFieldDefinition, fieldContext), contentPartFieldDefinition, fieldContext, _logger).ConfigureAwait(false);
            });
        }).ConfigureAwait(false);

        return partFieldDefinitionShape;
    }
}
