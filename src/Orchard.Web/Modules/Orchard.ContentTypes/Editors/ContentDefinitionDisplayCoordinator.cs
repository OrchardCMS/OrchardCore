using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;

using Orchard.DisplayManagement.ModelBinding;
using Orchard.DependencyInjection;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.Theming;
using Orchard.DisplayManagement.Layout;
using System;
using Orchard.ContentManagement.MetaData.Settings;
using Orchard.DisplayManagement.Views;
using Orchard.ContentTypes.ViewModels;
using Orchard.ContentManagement.Metadata.Settings;

namespace Orchard.ContentTypes.Editors
{

    public class UpdateContentDefinitionEditorContext<TBuilder> : UpdateEditorContext
    {
        public UpdateContentDefinitionEditorContext(
            TBuilder builder,
            IShape model,
            string groupId,
            IShapeFactory shapeFactory,
            IShape layout,
            IUpdateModel updater)
            : base(model, groupId, shapeFactory, layout, updater)
        {
            Builder = builder;
        }

        public TBuilder Builder { get; private set; }
    }

    public class UpdateTypeEditorContext : UpdateContentDefinitionEditorContext<ContentTypeDefinitionBuilder>
    {
        public UpdateTypeEditorContext(
                ContentTypeDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }

    public class UpdatePartEditorContext : UpdateContentDefinitionEditorContext<ContentTypePartDefinitionBuilder>
    {
        public UpdatePartEditorContext(
                ContentTypePartDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }

    public class UpdateFieldEditorContext : UpdateContentDefinitionEditorContext<ContentPartFieldDefinitionBuilder>
    {
        public UpdateFieldEditorContext(
                ContentPartFieldDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }

    public interface IContentDefinitionDisplayHandler : IDependency
    {
        Task BuildTypeDisplayAsync(ContentTypeDefinition definition, BuildDisplayContext context);
        Task BuildTypeEditorAsync(ContentTypeDefinition definition, BuildEditorContext context);
        Task UpdateTypeEditorAsync(ContentTypeDefinition definition, UpdateTypeEditorContext context);

        Task BuildPartDisplayAsync(ContentTypePartDefinition definition, BuildDisplayContext context);
        Task BuildPartEditorAsync(ContentTypePartDefinition definition, BuildEditorContext context);
        Task UpdatePartEditorAsync(ContentTypePartDefinition definition, UpdatePartEditorContext context);

        Task BuildFieldDisplayAsync(ContentPartFieldDefinition definition, BuildDisplayContext context);
        Task BuildFieldEditorAsync(ContentPartFieldDefinition definition, BuildEditorContext context);
        Task UpdateFieldEditorAsync(ContentPartFieldDefinition definition, UpdateFieldEditorContext context);
    }

    public interface IContentTypeDefinitionDisplayDriver : IDisplayDriver<ContentTypeDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypeEditorContext>, IDependency
    {
    }

    public interface IContentTypePartDefinitionDisplayDriver : IDisplayDriver<ContentTypePartDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartEditorContext>, IDependency
    {
    }

    public interface IContentPartFieldDefinitionDisplayDriver : IDisplayDriver<ContentPartFieldDefinition, BuildDisplayContext, BuildEditorContext, UpdateFieldEditorContext>, IDependency
    {
    }

    public class ContentDefinitionDisplayCoordinator : IContentDefinitionDisplayHandler
    {
        private readonly IEnumerable<IContentTypeDefinitionDisplayDriver> _typeDisplayDrivers;
        private readonly IEnumerable<IContentTypePartDefinitionDisplayDriver> _partDisplayDrivers;
        private readonly IEnumerable<IContentPartFieldDefinitionDisplayDriver> _fieldDisplayDrivers;

        public ContentDefinitionDisplayCoordinator(
            IEnumerable<IContentTypeDefinitionDisplayDriver> typeDisplayDrivers,
            IEnumerable<IContentTypePartDefinitionDisplayDriver> partDisplayDrivers,
            IEnumerable<IContentPartFieldDefinitionDisplayDriver> fieldDisplayDrivers,
            ILogger<IContentDefinitionDisplayHandler> logger)
        {
            _fieldDisplayDrivers = fieldDisplayDrivers;
            _partDisplayDrivers = partDisplayDrivers;
            _typeDisplayDrivers = typeDisplayDrivers;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

        public Task BuildTypeDisplayAsync(ContentTypeDefinition model, BuildDisplayContext context)
        {
            return _typeDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildDisplayAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildTypeEditorAsync(ContentTypeDefinition model, BuildEditorContext context)
        {
            return _typeDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateTypeEditorAsync(ContentTypeDefinition model, UpdateTypeEditorContext context)
        {
            return _typeDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildPartDisplayAsync(ContentTypePartDefinition model, BuildDisplayContext context)
        {
            return _partDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildDisplayAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildPartEditorAsync(ContentTypePartDefinition model, BuildEditorContext context)
        {
            return _partDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdatePartEditorAsync(ContentTypePartDefinition model, UpdatePartEditorContext context)
        {
            return _partDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildFieldDisplayAsync(ContentPartFieldDefinition model, BuildDisplayContext context)
        {
            return _fieldDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildDisplayAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildFieldEditorAsync(ContentPartFieldDefinition model, BuildEditorContext context)
        {
            return _fieldDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateFieldEditorAsync(ContentPartFieldDefinition model, UpdateFieldEditorContext context)
        {
            return _fieldDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }
    }

    public interface IContentDefinitionDisplayManager : IDependency
    {
        Task<dynamic> BuildDisplayAsync(ContentTypeDefinition definition, IUpdateModel updater, string displayType = "", string groupId = "");
        Task<dynamic> BuildEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdateEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupId = "");
    }

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

        public async Task<dynamic> BuildDisplayAsync(ContentTypeDefinition contentTypeDefinition, IUpdateModel updater, string displayType, string groupId)
        {
            if (contentTypeDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentTypeDefinition));
            }

            // [ContentDefinition]; [ContentDefinition_Summary];

            var actualShapeType = "ContentDefinition";

            var actualDisplayType = string.IsNullOrEmpty(displayType) ? "Detail" : displayType;

            if (actualDisplayType != "Detail")
            {
                actualShapeType = actualShapeType + "_" + actualDisplayType;
            }

            dynamic itemShape = CreateContentShape(actualShapeType);
            itemShape.ContentType = contentTypeDefinition;
            itemShape.Metadata.DisplayType = actualDisplayType;

            var context = new BuildDisplayContext(
                itemShape,
                actualDisplayType,
                groupId,
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _handlers.InvokeAsync(handler => handler.BuildTypeDisplayAsync(contentTypeDefinition, context), Logger);

            return context.Shape;
        }

        public async Task<dynamic> BuildEditorAsync(ContentTypeDefinition contentTypeDefinition, IUpdateModel updater, string groupId)
        {
            if (contentTypeDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentTypeDefinition));
            }

            var contentTypeDefinitionShape = _shapeFactory.Create<ContentTypeDefinitionViewModel>("ContentDefinition");
            contentTypeDefinitionShape.ContentTypeDefinition = contentTypeDefinition;
            contentTypeDefinitionShape.DisplayName = contentTypeDefinition.DisplayName;

            dynamic typeShape = CreateContentShape("ContentTypeDefinition_Edit");

            var typeContext = new BuildEditorContext(
                typeShape,
                groupId,
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            foreach (var contentPartDefinition in contentTypeDefinition.Parts)
            {
                dynamic partShape = CreateContentShape("ContentTypePartDefinition_Edit");
                partShape.ContentPart = contentPartDefinition;

                var partContext = new BuildEditorContext(
                    partShape,
                    groupId,
                    _shapeFactory,
                    _layoutAccessor.GetLayout(),
                    updater
                );

                await BindPlacementAsync(partContext);

                await _handlers.InvokeAsync(handler => handler.BuildPartEditorAsync(contentPartDefinition, partContext), Logger);

                contentTypeDefinitionShape.TypePartSettings.Add(partContext.Shape);

                // part fields
                //foreach (var field in part.PartDefinition.Fields)
                //    field.Templates = _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditor(field._Definition), Logger);
            }

            await BindPlacementAsync(typeContext);

            await _handlers.InvokeAsync(handler => handler.BuildTypeEditorAsync(contentTypeDefinition, typeContext), Logger);

            contentTypeDefinitionShape.TypeSettings = typeShape;

            // global fields
            //if (viewModel.Fields.Any())
            //{
            //    foreach (var field in viewModel.Fields)
            //        field.Templates = _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditor(field._Definition), Logger);
            //}

            return contentTypeDefinitionShape;
        }

        public Task<dynamic> UpdateEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupInfoId)
        {
            return Task.FromResult<dynamic>(null);

            //_contentDefinitionManager.AlterTypeDefinition(typeViewModel.Name, typeBuilder =>
            //{
            //    typeBuilder.DisplayedAs(typeViewModel.DisplayName);

            //    // allow extensions to alter type configuration
            //    _contentDefinitionEditorEvents.Invoke(x => x.TypeEditorUpdating(typeBuilder), Logger);
            //    typeViewModel.Templates = _contentDefinitionEditorEvents.Invoke(x => x.TypeEditorUpdate(typeBuilder, updater), Logger);
            //    _contentDefinitionEditorEvents.Invoke(x => x.TypeEditorUpdated(typeBuilder), Logger);

            //    foreach (var part in typeViewModel.Parts)
            //    {
            //        var partViewModel = part;

            //        // enable updater to be aware of changing part prefix
            //        updater.Prefix = secondHalf => String.Format("{0}.{1}", partViewModel.Prefix, secondHalf);

            //        // allow extensions to alter typePart configuration
            //        typeBuilder.WithPart(partViewModel.PartDefinition.Name, typePartBuilder =>
            //        {
            //            _contentDefinitionEditorEvents.Invoke(x => x.TypePartEditorUpdating(typePartBuilder), Logger);
            //            partViewModel.Templates = _contentDefinitionEditorEvents.Invoke(x => x.TypePartEditorUpdate(typePartBuilder, updater), Logger);
            //            _contentDefinitionEditorEvents.Invoke(x => x.TypePartEditorUpdated(typePartBuilder), Logger);
            //        });

            //        if (!partViewModel.PartDefinition.Fields.Any())
            //            continue;

            //        _contentDefinitionManager.AlterPartDefinition(partViewModel.PartDefinition.Name, partBuilder =>
            //        {
            //            var fieldFirstHalf = String.Format("{0}.{1}", partViewModel.Prefix, partViewModel.PartDefinition.Prefix);
            //            foreach (var field in partViewModel.PartDefinition.Fields)
            //            {
            //                var fieldViewModel = field;

            //                // enable updater to be aware of changing field prefix
            //                updater.Prefix = secondHalf =>
            //                    String.Format("{0}.{1}.{2}", fieldFirstHalf, fieldViewModel.Prefix, secondHalf);
            //                // allow extensions to alter partField configuration
            //                partBuilder.WithField(fieldViewModel.Name, partFieldBuilder =>
            //                {
            //                    _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdating(partFieldBuilder), Logger);
            //                    fieldViewModel.Templates = _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdate(partFieldBuilder, updater), Logger);
            //                    _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdated(partFieldBuilder), Logger);
            //                });
            //            }
            //        });
            //    }

            //    if (typeViewModel.Fields.Any())
            //    {
            //        _contentDefinitionManager.AlterPartDefinition(typeViewModel.Name, partBuilder =>
            //        {
            //            foreach (var field in typeViewModel.Fields)
            //            {
            //                var fieldViewModel = field;

            //                // enable updater to be aware of changing field prefix
            //                updater.Prefix = secondHalf =>
            //                    string.Format("{0}.{1}", fieldViewModel.Prefix, secondHalf);

            //                // allow extensions to alter partField configuration
            //                partBuilder.WithField(fieldViewModel.Name, partFieldBuilder =>
            //                {
            //                    _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdating(partFieldBuilder), Logger);
            //                    fieldViewModel.Templates = _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdate(partFieldBuilder, updater), Logger);
            //                    _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdated(partFieldBuilder), Logger);
            //                });
            //            }
            //        });
            //    }
            //});
        }
    }

    public abstract class ContentTypeDisplayDriver : DisplayDriver<ContentTypeDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypeEditorContext>, IContentTypeDefinitionDisplayDriver
    {
    }

    public abstract class ContentTypePartDisplayDriver : DisplayDriver<ContentTypePartDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartEditorContext>, IContentTypePartDefinitionDisplayDriver
    {
    }

    public abstract class ContentPartFieldDisplayDriver : DisplayDriver<ContentPartFieldDefinition, BuildDisplayContext, BuildEditorContext, UpdateFieldEditorContext>, IContentPartFieldDefinitionDisplayDriver
    {
    }

    public class ContentTypeSettingsDisplayDriver : ContentTypeDisplayDriver
    {

        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Shape<ContentTypeSettingsViewModel>("ContentTypeSettings_Edit", model =>
            {
                var settings = contentTypeDefinition.Settings.ToObject<ContentTypeSettings>();

                model.Creatable = settings.Creatable;
                model.Listable = settings.Listable;
                model.Draftable = settings.Draftable;
                model.Securable = settings.Securable;
                model.Stereotype = settings.Stereotype;

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
        {
            var model = new ContentTypeSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, "ContentTypeSettingsViewModel");

            context.Builder.Creatable(model.Creatable);
            context.Builder.Listable(model.Listable);
            context.Builder.Draftable(model.Draftable);
            context.Builder.Securable(model.Securable);
            context.Builder.Stereotype(model.Stereotype);

            return Edit(contentTypeDefinition, context.Updater);
        }
    }

    public class ContentPartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
        {
            return Shape("ContentTypePartSettings_Edit", new { ContentPart = model }).Location("Content");
        }
    }

}