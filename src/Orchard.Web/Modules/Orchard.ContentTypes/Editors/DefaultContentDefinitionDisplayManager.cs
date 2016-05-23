using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.ViewModels;
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

            var contentTypeDefinitionShape = _shapeFactory.Create<ContentTypeDefinitionViewModel>("ContentTypeDefinition");
            contentTypeDefinitionShape.ContentTypeDefinition = contentTypeDefinition;

            dynamic typeShape = CreateContentShape("ContentTypeDefinition_Edit");

            var typeContext = new BuildEditorContext(
                typeShape,
                groupId,
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                // Don't show the type's private part as it can't be removed or configured
                if (String.Equals(contentTypePartDefinition.PartDefinition.Name, contentTypeDefinition.Name, StringComparison.Ordinal))
                {
                    continue;
                }

                dynamic partShape = CreateContentShape("ContentTypePartDefinition_Edit");
                partShape.ContentPart = contentTypePartDefinition;

                var partContext = new BuildEditorContext(
                    partShape,
                    groupId,
                    _shapeFactory,
                    _layoutAccessor.GetLayout(),
                    updater
                );

                await BindPlacementAsync(partContext);

                await _handlers.InvokeAsync(handler => handler.BuildTypePartEditorAsync(contentTypePartDefinition, partContext), Logger);

                contentTypeDefinitionShape.TypePartSettings.Add(partContext.Shape);
            }

            await BindPlacementAsync(typeContext);

            await _handlers.InvokeAsync(handler => handler.BuildTypeEditorAsync(contentTypeDefinition, typeContext), Logger);

            contentTypeDefinitionShape.TypeSettings = typeShape;

            // Global fields

            var globalPartDefinition = _contentDefinitionManager.GetPartDefinition(contentTypeDefinition.Name);

            if (globalPartDefinition != null && globalPartDefinition.Fields.Any())
            {
                foreach (var contentPartFieldDefinition in globalPartDefinition.Fields)
                {
                    dynamic fieldShape = CreateContentShape("ContentPartFieldDefinition_Edit");
                    fieldShape.ContentField = contentPartFieldDefinition;

                    var fieldContext = new BuildEditorContext(
                        fieldShape,
                        groupId,
                        _shapeFactory,
                        _layoutAccessor.GetLayout(),
                        updater
                    );

                    BindPlacementAsync(fieldContext).Wait();

                    _handlers.InvokeAsync(handler => handler.BuildPartFieldEditorAsync(contentPartFieldDefinition, fieldContext), Logger).Wait();

                    contentTypeDefinitionShape.TypeFieldSettings.Add(fieldContext.Shape);
                }
            }

            return contentTypeDefinitionShape;
        }

        public Task<dynamic> UpdateTypeEditorAsync(ContentTypeDefinition contentTypeDefinition, IUpdateModel updater, string groupId)
        {
            if (contentTypeDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentTypeDefinition));
            }

            var contentTypeDefinitionShape = _shapeFactory.Create<ContentTypeDefinitionViewModel>("ContentTypeDefinition");

            _contentDefinitionManager.AlterTypeDefinition(contentTypeDefinition.Name, typeBuilder =>
            {
                contentTypeDefinitionShape.ContentTypeDefinition = contentTypeDefinition;

                dynamic typeShape = CreateContentShape("ContentTypeDefinition_Edit");

                var typeContext = new UpdateTypeEditorContext(
                    typeBuilder,
                    typeShape,
                    groupId,
                    _shapeFactory,
                    _layoutAccessor.GetLayout(),
                    updater
                );

                foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
                {
                // Don't show the type's private part as it can't be removed or configured
                if (String.Equals(contentTypePartDefinition.PartDefinition.Name, contentTypeDefinition.Name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    dynamic partShape = CreateContentShape("ContentTypePartDefinition_Edit");

                    typeBuilder.WithPart(contentTypePartDefinition.PartDefinition.Name, typePartBuilder =>
                    {
                        partShape.ContentPart = contentTypePartDefinition;

                        var partContext = new UpdateTypePartEditorContext(
                            typePartBuilder,
                            partShape,
                            groupId,
                            _shapeFactory,
                            _layoutAccessor.GetLayout(),
                            updater
                        );

                        BindPlacementAsync(partContext).Wait();

                        _handlers.InvokeAsync(handler => handler.UpdateTypePartEditorAsync(contentTypePartDefinition, partContext), Logger).Wait();

                        contentTypeDefinitionShape.TypePartSettings.Add(partContext.Shape);
                    });
                }

            // Global fields
            var globalPartDefinition = _contentDefinitionManager.GetPartDefinition(contentTypeDefinition.Name);

                if (globalPartDefinition != null && globalPartDefinition.Fields.Any())
                {
                    _contentDefinitionManager.AlterPartDefinition(globalPartDefinition.Name, partBuilder =>
                    {
                        foreach (var contentPartFieldDefinition in globalPartDefinition.Fields)
                        {
                            dynamic fieldShape = CreateContentShape("ContentPartFieldDefinition_Edit");

                            partBuilder.WithField(contentPartFieldDefinition.Name, partFieldBuilder =>
                            {
                                fieldShape.ContentField = contentPartFieldDefinition;

                                var fieldContext = new UpdatePartFieldEditorContext(
                                    partFieldBuilder,
                                    fieldShape,
                                    groupId,
                                    _shapeFactory,
                                    _layoutAccessor.GetLayout(),
                                    updater
                                );

                                BindPlacementAsync(fieldContext).Wait();

                                _handlers.InvokeAsync(handler => handler.UpdatePartFieldEditorAsync(contentPartFieldDefinition, fieldContext), Logger).Wait();

                                contentTypeDefinitionShape.TypeFieldSettings.Add(fieldContext.Shape);

                            });
                        }
                    });
                }

                BindPlacementAsync(typeContext).Wait();

                _handlers.InvokeAsync(handler => handler.UpdateTypeEditorAsync(contentTypeDefinition, typeContext), Logger).Wait();

                contentTypeDefinitionShape.TypeSettings = typeShape;

            });

            return Task.FromResult<dynamic>(contentTypeDefinitionShape);
        }

        public async Task<dynamic> BuildPartEditorAsync(ContentPartDefinition contentPartDefinition, IUpdateModel updater, string groupId)
        {
            if (contentPartDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentPartDefinition));
            }

            var contentPartDefinitionShape = _shapeFactory.Create<ContentPartDefinitionViewModel>("ContentPartDefinition");
            contentPartDefinitionShape.ContentPartDefinition = contentPartDefinition;

            dynamic partShape = CreateContentShape("ContentPartDefinition_Edit");

            var partContext = new BuildEditorContext(
                partShape,
                groupId,
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            foreach (var contentPartFieldDefinition in contentPartDefinition.Fields)
            {
                dynamic fieldShape = CreateContentShape("ContentPartFieldDefinition_Edit");
                fieldShape.ContentField = contentPartFieldDefinition;

                var fieldContext = new BuildEditorContext(
                    fieldShape,
                    groupId,
                    _shapeFactory,
                    _layoutAccessor.GetLayout(),
                    updater
                );

                BindPlacementAsync(fieldContext).Wait();

                _handlers.InvokeAsync(handler => handler.BuildPartFieldEditorAsync(contentPartFieldDefinition, fieldContext), Logger).Wait();

                contentPartDefinitionShape.PartFieldSettings.Add(fieldContext.Shape);
            }

            await BindPlacementAsync(partContext);

            await _handlers.InvokeAsync(handler => handler.BuildPartEditorAsync(contentPartDefinition, partContext), Logger);

            contentPartDefinitionShape.PartSettings = partShape;

            return contentPartDefinitionShape;
        }

        public async Task<dynamic> UpdatePartEditorAsync(ContentPartDefinition contentPartDefinition, IUpdateModel updater, string groupId)
        {
            if (contentPartDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentPartDefinition));
            }

            var contentPartDefinitionShape = _shapeFactory.Create<ContentPartDefinitionViewModel>("ContentPartDefinition");

            contentPartDefinitionShape.ContentPartDefinition = contentPartDefinition;

            dynamic partShape = CreateContentShape("ContentPartDefinition_Edit");

            UpdatePartEditorContext partContext = null;

            _contentDefinitionManager.AlterPartDefinition(contentPartDefinition.Name, partBuilder =>
            {
                partContext = new UpdatePartEditorContext(
                    partBuilder,
                    partShape,
                    groupId,
                    _shapeFactory,
                    _layoutAccessor.GetLayout(),
                    updater
                );

                foreach (var contentFieldDefinition in contentPartDefinition.Fields)
                {
                    dynamic fieldShape = CreateContentShape("ContentPartFieldDefinition_Edit");

                    partBuilder.WithField(contentFieldDefinition.Name, partFieldBuilder =>
                    {
                        fieldShape.ContentField = contentFieldDefinition;

                        var fieldContext = new UpdatePartFieldEditorContext(
                            partFieldBuilder,
                            fieldShape,
                            groupId,
                            _shapeFactory,
                            _layoutAccessor.GetLayout(),
                            updater
                        );

                        BindPlacementAsync(fieldContext).Wait();

                        _handlers.InvokeAsync(handler => handler.UpdatePartFieldEditorAsync(contentFieldDefinition, fieldContext), Logger).Wait();

                        contentPartDefinitionShape.PartFieldSettings.Add(fieldContext.Shape);

                    });
                }
            });

            await BindPlacementAsync(partContext);

            _handlers.InvokeAsync(handler => handler.UpdatePartEditorAsync(contentPartDefinition, partContext), Logger).Wait();

            contentPartDefinitionShape.PartSettings = partShape;

            return contentPartDefinitionShape;
        }
    }
}