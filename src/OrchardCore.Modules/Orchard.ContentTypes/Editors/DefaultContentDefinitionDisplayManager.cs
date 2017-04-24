using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Theming;

namespace Orchard.ContentTypes.Editors
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

            dynamic contentTypeDefinitionShape = CreateContentShape("ContentTypeDefinition_Edit");
            contentTypeDefinitionShape.ContentTypeDefinition = contentTypeDefinition;

            var typeContext = new BuildEditorContext(
                contentTypeDefinitionShape,
                groupId,
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(typeContext);

            await _handlers.InvokeAsync(handler => handler.BuildTypeEditorAsync(contentTypeDefinition, typeContext), Logger);

            return contentTypeDefinitionShape;
        }

        public Task<dynamic> UpdateTypeEditorAsync(ContentTypeDefinition contentTypeDefinition, IUpdateModel updater, string groupId)
        {
            if (contentTypeDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentTypeDefinition));
            }

            dynamic contentTypeDefinitionShape = CreateContentShape("ContentTypeDefinition_Edit");
            contentTypeDefinitionShape.ContentTypeDefinition = contentTypeDefinition;

            _contentDefinitionManager.AlterTypeDefinition(contentTypeDefinition.Name, typeBuilder =>
            {
                var typeContext = new UpdateTypeEditorContext(
                    typeBuilder,
                    contentTypeDefinitionShape,
                    groupId,
                    _shapeFactory,
                    _layoutAccessor.GetLayout(),
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

            var contentPartDefinitionShape = CreateContentShape("ContentPartDefinition_Edit");

            var partContext = new BuildEditorContext(
                contentPartDefinitionShape,
                groupId,
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
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

            var contentPartDefinitionShape = CreateContentShape("ContentPartDefinition_Edit");

            UpdatePartEditorContext partContext = null;

            _contentDefinitionManager.AlterPartDefinition(contentPartDefinition.Name, partBuilder =>
            {
                partContext = new UpdatePartEditorContext(
                    partBuilder,
                    contentPartDefinitionShape,
                    groupId,
                    _shapeFactory,
                    _layoutAccessor.GetLayout(),
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

            dynamic typePartDefinitionShape = CreateContentShape("ContentTypePartDefinition_Edit");
            typePartDefinitionShape.ContentPart = contentTypePartDefinition;

            var partContext = new BuildEditorContext(
                typePartDefinitionShape,
                groupId,
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(partContext);

            await _handlers.InvokeAsync(handler => handler.BuildTypePartEditorAsync(contentTypePartDefinition, partContext), Logger);


            return typePartDefinitionShape;
        }

        public Task<dynamic> UpdateTypePartEditorAsync(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater, string groupId = "")
        {
            if (contentTypePartDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentTypePartDefinition));
            }

            dynamic typePartDefinitionShape = CreateContentShape("ContentTypePartDefinition_Edit");

            _contentDefinitionManager.AlterTypeDefinition(contentTypePartDefinition.ContentTypeDefinition.Name, typeBuilder =>
            {

                typeBuilder.WithPart(contentTypePartDefinition.Name, async typePartBuilder =>
                {
                    typePartDefinitionShape.ContentPart = contentTypePartDefinition;

                    var partContext = new UpdateTypePartEditorContext(
                        typePartBuilder,
                        typePartDefinitionShape,
                        groupId,
                        _shapeFactory,
                        _layoutAccessor.GetLayout(),
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

            dynamic partFieldDefinitionShape = CreateContentShape("ContentPartFieldDefinition_Edit");
            partFieldDefinitionShape.ContentField = contentPartFieldDefinition;

            var fieldContext = new BuildEditorContext(
                partFieldDefinitionShape,
                groupId,
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(fieldContext);

            await _handlers.InvokeAsync(handler => handler.BuildPartFieldEditorAsync(contentPartFieldDefinition, fieldContext), Logger);

            return partFieldDefinitionShape;
        }

        public Task<dynamic> UpdatePartFieldEditorAsync(ContentPartFieldDefinition contentPartFieldDefinition, IUpdateModel updater, string groupId = "")
        {
            if (contentPartFieldDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentPartFieldDefinition));
            }

            var contentPartDefinition = contentPartFieldDefinition.PartDefinition;
            dynamic partFieldDefinitionShape = CreateContentShape("ContentPartFieldDefinition_Edit");

            _contentDefinitionManager.AlterPartDefinition(contentPartDefinition.Name, partBuilder =>
            {
                partBuilder.WithField(contentPartFieldDefinition.Name, async partFieldBuilder =>
                {
                    partFieldDefinitionShape.ContentField = contentPartFieldDefinition;

                    var fieldContext = new UpdatePartFieldEditorContext(
                        partFieldBuilder,
                        partFieldDefinitionShape,
                        groupId,
                        _shapeFactory,
                        _layoutAccessor.GetLayout(),
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