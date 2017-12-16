using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Modules;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.ContentTypes.Editors
{
    public class DefaultContentDefinitionDisplayManager : BaseDisplayManager, IContentDefinitionDisplayManager
    {
        private readonly IEnumerable<IContentDefinitionDisplayHandler> _handlers;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IThemeManager _themeManager;
        private readonly ILayoutAccessor _layoutAccessor;

        public DefaultContentDefinitionDisplayManager(
            IEnumerable<IContentDefinitionDisplayHandler> handlers,
            IShapeTableManager shapeTableManager,
            IContentDefinitionManager contentDefinitionManager,
            IShapeFactory shapeFactory,
            IThemeManager themeManager,
            ILogger<DefaultContentDefinitionDisplayManager> logger,
            ILayoutAccessor layoutAccessor
            ) : base(shapeTableManager, shapeFactory, themeManager)
        {
            _handlers = handlers;
            _shapeTableManager = shapeTableManager;
            _contentDefinitionManager = contentDefinitionManager;
            _shapeFactory = shapeFactory;
            _themeManager = themeManager;
            _layoutAccessor = layoutAccessor;

            Logger = logger;
        }

        ILogger Logger { get; set; }

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

            await _handlers.InvokeAsync(handler => handler.BuildTypeEditorAsync(contentTypeDefinition, typeContext), Logger);

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

            _contentDefinitionManager.AlterTypeDefinition(contentTypeDefinition.Name, typeBuilder =>
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

                BindPlacementAsync(typeContext).Wait();

                _handlers.InvokeAsync(handler => handler.UpdateTypeEditorAsync(contentTypeDefinition, typeContext), Logger).Wait();

            });

            return Task.FromResult<dynamic>(contentTypeDefinitionShape);
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

            await _handlers.InvokeAsync(async handler => await handler.BuildPartEditorAsync(contentPartDefinition, partContext), Logger);

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

            _contentDefinitionManager.AlterPartDefinition(contentPartDefinition.Name, partBuilder =>
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
            });

            await BindPlacementAsync(partContext);

            await _handlers.InvokeAsync(handler => handler.UpdatePartEditorAsync(contentPartDefinition, partContext), Logger);

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

            await _handlers.InvokeAsync(handler => handler.BuildTypePartEditorAsync(contentTypePartDefinition, partContext), Logger);


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

            _contentDefinitionManager.AlterTypeDefinition(contentTypePartDefinition.ContentTypeDefinition.Name, typeBuilder =>
            {

                typeBuilder.WithPart(contentTypePartDefinition.Name, async typePartBuilder =>
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

                    await _handlers.InvokeAsync(handler => handler.UpdateTypePartEditorAsync(contentTypePartDefinition, partContext), Logger);
                });

            });

            return Task.FromResult<dynamic>(typePartDefinitionShape);
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

            await _handlers.InvokeAsync(handler => handler.BuildPartFieldEditorAsync(contentPartFieldDefinition, fieldContext), Logger);

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

            _contentDefinitionManager.AlterPartDefinition(contentPartDefinition.Name, partBuilder =>
            {
                partBuilder.WithField(contentPartFieldDefinition.Name, async partFieldBuilder =>
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

                    await _handlers.InvokeAsync(handler => handler.UpdatePartFieldEditorAsync(contentPartFieldDefinition, fieldContext), Logger);
                });
            });

            return Task.FromResult<dynamic>(partFieldDefinitionShape);
        }
    }
}