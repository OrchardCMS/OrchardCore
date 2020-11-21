using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;

namespace OrchardCore.ContentTypes.Editors
{
    public class DefaultContentDefinitionDisplayManager : BaseDisplayManager, IContentDefinitionDisplayManager
    {
        private readonly IEnumerable<IContentDefinitionDisplayHandler> _handlers;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly ILogger _logger;

        public DefaultContentDefinitionDisplayManager(
            IEnumerable<IContentDefinitionDisplayHandler> handlers,
            IShapeTableManager shapeTableManager,
            IContentDefinitionManager contentDefinitionManager,
            IShapeFactory shapeFactory,
            IEnumerable<IShapePlacementProvider> placementProviders,
            ILogger<DefaultContentDefinitionDisplayManager> logger,
            ILayoutAccessor layoutAccessor
            ) : base(shapeFactory, placementProviders)
        {
            _handlers = handlers;
            _shapeTableManager = shapeTableManager;
            _contentDefinitionManager = contentDefinitionManager;
            _shapeFactory = shapeFactory;
            _layoutAccessor = layoutAccessor;
            _logger = logger;
        }

        public async Task<dynamic> BuildTypeEditorAsync(ContentTypeDefinition contentTypeDefinition, IUpdateModel updater, string groupId)
        {
            if (contentTypeDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentTypeDefinition));
            }

            dynamic contentTypeDefinitionShape = await CreateContentShapeAsync("ContentTypeDefinition_Edit");
            contentTypeDefinitionShape.ContentTypeDefinition = contentTypeDefinition;

            var typeContext = new BuildEditorContext(
                contentTypeDefinitionShape,
                groupId,
                false,
                "",
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                updater
            );

            await BindPlacementAsync(typeContext);

            await _handlers.InvokeAsync((handler, contentTypeDefinition, typeContext) => handler.BuildTypeEditorAsync(contentTypeDefinition, typeContext), contentTypeDefinition, typeContext, _logger);

            return contentTypeDefinitionShape;
        }

        public async Task<dynamic> UpdateTypeEditorAsync(ContentTypeDefinition contentTypeDefinition, IUpdateModel updater, string groupId)
        {
            if (contentTypeDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentTypeDefinition));
            }

            dynamic contentTypeDefinitionShape = await CreateContentShapeAsync("ContentTypeDefinition_Edit");
            contentTypeDefinitionShape.ContentTypeDefinition = contentTypeDefinition;

            var layout = await _layoutAccessor.GetLayoutAsync();

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

                await BindPlacementAsync(typeContext);

                await _handlers.InvokeAsync((handler, contentTypeDefinition, typeContext) => handler.UpdateTypeEditorAsync(contentTypeDefinition, typeContext), contentTypeDefinition, typeContext, _logger);
            });

            return contentTypeDefinitionShape;
        }

        public async Task<dynamic> BuildPartEditorAsync(ContentPartDefinition contentPartDefinition, IUpdateModel updater, string groupId)
        {
            if (contentPartDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentPartDefinition));
            }

            var contentPartDefinitionShape = await CreateContentShapeAsync("ContentPartDefinition_Edit");

            var partContext = new BuildEditorContext(
                contentPartDefinitionShape,
                groupId,
                false,
                "",
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                updater
            );

            await BindPlacementAsync(partContext);

            await _handlers.InvokeAsync((handler, contentPartDefinition, partContext) => handler.BuildPartEditorAsync(contentPartDefinition, partContext), contentPartDefinition, partContext, _logger);

            return contentPartDefinitionShape;
        }

        public async Task<dynamic> UpdatePartEditorAsync(ContentPartDefinition contentPartDefinition, IUpdateModel updater, string groupId)
        {
            if (contentPartDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentPartDefinition));
            }

            var contentPartDefinitionShape = await CreateContentShapeAsync("ContentPartDefinition_Edit");

            UpdatePartEditorContext partContext = null;
            var layout = await _layoutAccessor.GetLayoutAsync();

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

                await BindPlacementAsync(partContext);

                await _handlers.InvokeAsync((handler, contentPartDefinition, partContext) => handler.UpdatePartEditorAsync(contentPartDefinition, partContext), contentPartDefinition, partContext, _logger);
            });

            return contentPartDefinitionShape;
        }

        public async Task<dynamic> BuildTypePartEditorAsync(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater, string groupId = "")
        {
            if (contentTypePartDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentTypePartDefinition));
            }

            dynamic typePartDefinitionShape = await CreateContentShapeAsync("ContentTypePartDefinition_Edit");
            typePartDefinitionShape.ContentPart = contentTypePartDefinition;

            var partContext = new BuildEditorContext(
                typePartDefinitionShape,
                groupId,
                false,
                "",
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                updater
            );

            await BindPlacementAsync(partContext);

            await _handlers.InvokeAsync((handler, contentTypePartDefinition, partContext) => handler.BuildTypePartEditorAsync(contentTypePartDefinition, partContext), contentTypePartDefinition, partContext, _logger);

            return typePartDefinitionShape;
        }

        public async Task<dynamic> UpdateTypePartEditorAsync(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater, string groupId = "")
        {
            if (contentTypePartDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentTypePartDefinition));
            }

            dynamic typePartDefinitionShape = await CreateContentShapeAsync("ContentTypePartDefinition_Edit");
            var layout = await _layoutAccessor.GetLayoutAsync();

            await _contentDefinitionManager.AlterTypeDefinitionAsync(contentTypePartDefinition.ContentTypeDefinition.Name, typeBuilder =>
            {
                return typeBuilder.WithPartAsync(contentTypePartDefinition.Name, async typePartBuilder =>
                {
                    typePartDefinitionShape.ContentPart = contentTypePartDefinition;

                    var partContext = new UpdateTypePartEditorContext(
                        typePartBuilder,
                        typePartDefinitionShape,
                        groupId,
                        false,
                        _shapeFactory,
                        layout,
                        updater
                    );

                    await BindPlacementAsync(partContext);

                    await _handlers.InvokeAsync((handler, contentTypePartDefinition, partContext) => handler.UpdateTypePartEditorAsync(contentTypePartDefinition, partContext), contentTypePartDefinition, partContext, _logger);
                });
            });

            return typePartDefinitionShape;
        }

        public async Task<dynamic> BuildPartFieldEditorAsync(ContentPartFieldDefinition contentPartFieldDefinition, IUpdateModel updater, string groupId = "")
        {
            if (contentPartFieldDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentPartFieldDefinition));
            }

            dynamic partFieldDefinitionShape = await CreateContentShapeAsync("ContentPartFieldDefinition_Edit");
            partFieldDefinitionShape.ContentField = contentPartFieldDefinition;

            var fieldContext = new BuildEditorContext(
                partFieldDefinitionShape,
                groupId,
                false,
                "",
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                updater
            );

            await BindPlacementAsync(fieldContext);

            await _handlers.InvokeAsync((handler, contentPartFieldDefinition, fieldContext) => handler.BuildPartFieldEditorAsync(contentPartFieldDefinition, fieldContext), contentPartFieldDefinition, fieldContext, _logger);

            return partFieldDefinitionShape;
        }

        public async Task<dynamic> UpdatePartFieldEditorAsync(ContentPartFieldDefinition contentPartFieldDefinition, IUpdateModel updater, string groupId = "")
        {
            if (contentPartFieldDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentPartFieldDefinition));
            }

            var contentPartDefinition = contentPartFieldDefinition.PartDefinition;
            dynamic partFieldDefinitionShape = await CreateContentShapeAsync("ContentPartFieldDefinition_Edit");

            var layout = await _layoutAccessor.GetLayoutAsync();

            await _contentDefinitionManager.AlterPartDefinitionAsync(contentPartDefinition.Name, partBuilder =>
            {
                return partBuilder.WithFieldAsync(contentPartFieldDefinition.Name, async partFieldBuilder =>
                {
                    partFieldDefinitionShape.ContentField = contentPartFieldDefinition;

                    var fieldContext = new UpdatePartFieldEditorContext(
                        partFieldBuilder,
                        partFieldDefinitionShape,
                        groupId,
                        false,
                        _shapeFactory,
                        layout,
                        updater
                    );

                    await BindPlacementAsync(fieldContext);

                    await _handlers.InvokeAsync((handler, contentPartFieldDefinition, fieldContext) => handler.UpdatePartFieldEditorAsync(contentPartFieldDefinition, fieldContext), contentPartFieldDefinition, fieldContext, _logger);
                });
            });

            return partFieldDefinitionShape;
        }
    }
}
