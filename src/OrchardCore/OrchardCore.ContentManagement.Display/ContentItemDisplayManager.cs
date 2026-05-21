using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Display;

/// <summary>
/// The default implementation of <see cref="IContentItemDisplayManager"/>. It is used to render
/// <see cref="ContentItem"/> objects by leveraging any <see cref="IContentDisplayHandler"/>
/// implementation. The resulting shapes are targeting the stereotype of the content item
/// to display.
/// </summary>
public class ContentItemDisplayManager : BaseDisplayManager, IContentItemDisplayManager
{
    private readonly IEnumerable<IContentDisplayHandler> _handlers;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly ILogger _logger;

    public ContentItemDisplayManager(
        IEnumerable<IContentDisplayHandler> handlers,
        IContentDefinitionManager contentDefinitionManager,
        IShapeFactory shapeFactory,
        IEnumerable<IShapePlacementProvider> placementProviders,
        ILogger<ContentItemDisplayManager> logger,
        ILayoutAccessor layoutAccessor
        ) : base(shapeFactory, placementProviders)
    {
        _handlers = handlers;
        _contentDefinitionManager = contentDefinitionManager;
        _shapeFactory = shapeFactory;
        _layoutAccessor = layoutAccessor;
        _logger = logger;
    }

    public async Task<IShape> BuildDisplayAsync(ContentItem contentItem, IUpdateModel updater, string displayType, string groupId)
    {
        ArgumentNullException.ThrowIfNull(contentItem);

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType)
            ?? throw new NullReferenceException($"Content Type {contentItem.ContentType} does not exist.");

        var actualDisplayType = string.IsNullOrEmpty(displayType) ? OrchardCoreConstants.DisplayType.Detail : displayType;
        var hasStereotype = contentTypeDefinition.TryGetStereotype(out var stereotype);

        var actualShapeType = "Content";

        if (hasStereotype)
        {
            actualShapeType = contentTypeDefinition.GetStereotype();
        }

        // [DisplayType] is only added for the ones different than Detail
        if (actualDisplayType != OrchardCoreConstants.DisplayType.Detail)
        {
            actualShapeType = actualShapeType + "_" + actualDisplayType;
        }

        var itemShape = await CreateContentShapeAsync(actualShapeType);
        itemShape.Properties["ContentItem"] = contentItem;
        itemShape.Properties["Stereotype"] = stereotype;

        var metadata = itemShape.Metadata;
        metadata.DisplayType = actualDisplayType;

        // Get cached alternates and add them efficiently
        var cachedAlternates = ContentItemAlternatesFactory.GetDisplayAlternates(
            stereotype,
            hasStereotype,
            actualDisplayType,
            actualShapeType,
            contentItem.ContentType);

        metadata.Alternates.AddRange(cachedAlternates);

        var context = new BuildDisplayContext(
            itemShape,
            actualDisplayType,
            groupId,
            _shapeFactory,
            await _layoutAccessor.GetLayoutAsync(),
            new ModelStateWrapperUpdater(updater)
        );

        await BindPlacementAsync(context);

        await _handlers.InvokeAsync((handler, contentItem, context) => handler.BuildDisplayAsync(contentItem, context), contentItem, context, _logger);

        return context.Shape;
    }

    public async Task<IShape> BuildEditorAsync(ContentItem contentItem, IUpdateModel updater, bool isNew, string groupId, string htmlFieldPrefix)
    {
        ArgumentNullException.ThrowIfNull(contentItem);

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType)
            ?? throw new NullReferenceException($"Content Type {contentItem.ContentType} does not exist.");

        var hasStereotype = contentTypeDefinition.TryGetStereotype(out var stereotype);
        var actualShapeType = "Content_Edit";

        if (hasStereotype)
        {
            actualShapeType = stereotype + "_Edit";
        }

        var itemShape = await CreateContentShapeAsync(actualShapeType);
        itemShape.Properties["ContentItem"] = contentItem;
        itemShape.Properties["Stereotype"] = stereotype;

        // Get cached alternates and add them efficiently
        var cachedAlternates = ContentItemAlternatesFactory.GetEditorAlternates(
            stereotype,
            hasStereotype,
            actualShapeType,
            contentItem.ContentType);

        itemShape.Metadata.Alternates.AddRange(cachedAlternates);

        var context = new BuildEditorContext(
            itemShape,
            groupId,
            isNew,
            htmlFieldPrefix,
            _shapeFactory,
            await _layoutAccessor.GetLayoutAsync(),
            new ModelStateWrapperUpdater(updater)
        );

        await BindPlacementAsync(context);

        await _handlers.InvokeAsync((handler, contentItem, context) => handler.BuildEditorAsync(contentItem, context), contentItem, context, _logger);

        return context.Shape;
    }

    public async Task<IShape> UpdateEditorAsync(ContentItem contentItem, IUpdateModel updater, bool isNew, string groupId, string htmlFieldPrefix)
    {
        ArgumentNullException.ThrowIfNull(contentItem);

        var contentTypeDefinition = await _contentDefinitionManager.LoadTypeDefinitionAsync(contentItem.ContentType)
            ?? throw new NullReferenceException($"Content Type {contentItem.ContentType} does not exist.");

        var hasStereotype = contentTypeDefinition.TryGetStereotype(out var stereotype);
        var actualShapeType = "Content_Edit";

        if (hasStereotype)
        {
            actualShapeType = stereotype + "_Edit";
        }

        var itemShape = await CreateContentShapeAsync(actualShapeType);
        itemShape.Properties["ContentItem"] = contentItem;
        itemShape.Properties["Stereotype"] = stereotype;

        // Get cached alternates and add them efficiently
        var cachedAlternates = ContentItemAlternatesFactory.GetEditorAlternates(
            stereotype,
            hasStereotype,
            actualShapeType,
            contentItem.ContentType);

        itemShape.Metadata.Alternates.AddRange(cachedAlternates);

        var context = new UpdateEditorContext(
            itemShape,
            groupId,
            isNew,
            htmlFieldPrefix,
            _shapeFactory,
            await _layoutAccessor.GetLayoutAsync(),
            new ModelStateWrapperUpdater(updater)
        );

        await BindPlacementAsync(context);

        var updateContentContext = new UpdateContentContext(contentItem);

        await _handlers.InvokeAsync((handler, contentItem, context) => handler.UpdateEditorAsync(contentItem, context), contentItem, context, _logger);

        return context.Shape;
    }
}
