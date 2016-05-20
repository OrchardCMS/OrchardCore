using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Settings;
using Orchard.ContentTypes.ViewModels;
using Orchard.DependencyInjection;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Theming;
using Orchard.DisplayManagement.Views;

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

    public class UpdateTypePartEditorContext : UpdateContentDefinitionEditorContext<ContentTypePartDefinitionBuilder>
    {
        public UpdateTypePartEditorContext(
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

    public class UpdatePartEditorContext : UpdateContentDefinitionEditorContext<ContentPartDefinitionBuilder>
    {
        public UpdatePartEditorContext(
                ContentPartDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }

    public class UpdatePartFieldEditorContext : UpdateContentDefinitionEditorContext<ContentPartFieldDefinitionBuilder>
    {
        public UpdatePartFieldEditorContext(
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
        Task BuildTypeEditorAsync(ContentTypeDefinition definition, BuildEditorContext context);
        Task UpdateTypeEditorAsync(ContentTypeDefinition definition, UpdateTypeEditorContext context);

        Task BuildTypePartEditorAsync(ContentTypePartDefinition definition, BuildEditorContext context);
        Task UpdateTypePartEditorAsync(ContentTypePartDefinition definition, UpdateTypePartEditorContext context);

        Task BuildPartEditorAsync(ContentPartDefinition definition, BuildEditorContext context);
        Task UpdatePartEditorAsync(ContentPartDefinition definition, UpdatePartEditorContext context);

        Task BuildPartFieldEditorAsync(ContentPartFieldDefinition definition, BuildEditorContext context);
        Task UpdatePartFieldEditorAsync(ContentPartFieldDefinition definition, UpdatePartFieldEditorContext context);
    }

    public interface IContentTypeDefinitionDisplayDriver : IDisplayDriver<ContentTypeDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypeEditorContext>, IDependency
    {
    }

    public interface IContentTypePartDefinitionDisplayDriver : IDisplayDriver<ContentTypePartDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypePartEditorContext>, IDependency
    {
    }

    public interface IContentPartDefinitionDisplayDriver : IDisplayDriver<ContentPartDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartEditorContext>, IDependency
    {
    }

    public interface IContentPartFieldDefinitionDisplayDriver : IDisplayDriver<ContentPartFieldDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartFieldEditorContext>, IDependency
    {
    }

    public class ContentDefinitionDisplayCoordinator : IContentDefinitionDisplayHandler
    {
        private readonly IEnumerable<IContentTypeDefinitionDisplayDriver> _typeDisplayDrivers;
        private readonly IEnumerable<IContentTypePartDefinitionDisplayDriver> _typePartDisplayDrivers;
        private readonly IEnumerable<IContentPartDefinitionDisplayDriver> _partDisplayDrivers;
        private readonly IEnumerable<IContentPartFieldDefinitionDisplayDriver> _partFieldDisplayDrivers;

        public ContentDefinitionDisplayCoordinator(
            IEnumerable<IContentTypeDefinitionDisplayDriver> typeDisplayDrivers,
            IEnumerable<IContentTypePartDefinitionDisplayDriver> typePartDisplayDrivers,
            IEnumerable<IContentPartDefinitionDisplayDriver> partDisplayDrivers,
            IEnumerable<IContentPartFieldDefinitionDisplayDriver> partFieldDisplayDrivers,
            ILogger<IContentDefinitionDisplayHandler> logger)
        {
            _partFieldDisplayDrivers = partFieldDisplayDrivers;
            _partDisplayDrivers = partDisplayDrivers;
            _typePartDisplayDrivers = typePartDisplayDrivers;
            _typeDisplayDrivers = typeDisplayDrivers;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

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

        public Task BuildTypePartEditorAsync(ContentTypePartDefinition model, BuildEditorContext context)
        {
            return _typePartDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateTypePartEditorAsync(ContentTypePartDefinition model, UpdateTypePartEditorContext context)
        {
            return _typePartDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildPartEditorAsync(ContentPartDefinition model, BuildEditorContext context)
        {
            return _partDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdatePartEditorAsync(ContentPartDefinition model, UpdatePartEditorContext context)
        {
            return _partDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildPartFieldEditorAsync(ContentPartFieldDefinition model, BuildEditorContext context)
        {
            return _partFieldDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdatePartFieldEditorAsync(ContentPartFieldDefinition model, UpdatePartFieldEditorContext context)
        {
            return _partFieldDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }
    }

    public interface IContentDefinitionDisplayManager : IDependency
    {
        Task<dynamic> BuildTypeEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdateTypeEditorAsync(ContentTypeDefinition definition, IUpdateModel updater, string groupId = "");

        Task<dynamic> BuildPartEditorAsync(ContentPartDefinition definition, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdatePartEditorAsync(ContentPartDefinition definition, IUpdateModel updater, string groupId = "");
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

    public abstract class ContentTypeDisplayDriver : DisplayDriver<ContentTypeDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypeEditorContext>, IContentTypeDefinitionDisplayDriver
    {
        public override string GeneratePrefix(ContentTypeDefinition model)
        {
            return model.Name;
        }
    }

    public abstract class ContentPartDisplayDriver : DisplayDriver<ContentPartDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartEditorContext>, IContentPartDefinitionDisplayDriver
    {
        public override string GeneratePrefix(ContentPartDefinition model)
        {
            return model.Name;
        }
    }

    public abstract class ContentTypePartDisplayDriver : DisplayDriver<ContentTypePartDefinition, BuildDisplayContext, BuildEditorContext, UpdateTypePartEditorContext>, IContentTypePartDefinitionDisplayDriver
    {
        public override string GeneratePrefix(ContentTypePartDefinition model)
        {
            return $"{model.ContentTypeDefinition.Name}.{model.PartDefinition.Name}";
        }
    }

    public abstract class ContentPartFieldDisplayDriver : DisplayDriver<ContentPartFieldDefinition, BuildDisplayContext, BuildEditorContext, UpdatePartFieldEditorContext>, IContentPartFieldDefinitionDisplayDriver
    {
        public override string GeneratePrefix(ContentPartFieldDefinition model)
        {
            return $"{model.PartDefinition.Name}.{model.Name}";
        }
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
            }).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
        {
            var model = new ContentTypeSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.Creatable(model.Creatable);
                context.Builder.Listable(model.Listable);
                context.Builder.Draftable(model.Draftable);
                context.Builder.Securable(model.Securable);
                context.Builder.Stereotype(model.Stereotype);
            }

            return Edit(contentTypeDefinition, context.Updater);
        }
    }

    public class ContentPartSettingsDisplayDriver : ContentPartDisplayDriver
    {

        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        {
            return Shape<ContentPartSettingsViewModel>("ContentPartSettings_Edit", model =>
            {
                var settings = contentPartDefinition.Settings.ToObject<ContentPartSettings>();

                model.Attachable = settings.Attachable;
                model.Description = settings.Description;

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartDefinition contentPartDefinition, UpdatePartEditorContext context)
        {
            var model = new ContentPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.Attachable(model.Attachable);
                context.Builder.WithDescription(model.Description);
            }

            return Edit(contentPartDefinition, context.Updater);
        }
    }

    public class DefaultContentTypeDisplayDriver : ContentTypeDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public DefaultContentTypeDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<DefaultContentDefinitionDisplayManager> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Shape<ContentTypeViewModel>("ContentType_Edit", model =>
            {
                model.DisplayName = contentTypeDefinition.DisplayName;
                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
        {
            var model = new ContentTypeViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.DisplayedAs(model.DisplayName);

            if (String.IsNullOrWhiteSpace(model.DisplayName))
            {
                context.Updater.ModelState.AddModelError("DisplayName", T["The Content Type name can't be empty."]);
            }

            return Edit(contentTypeDefinition, context.Updater);
        }
    }

    public class ContentTypePartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
        {
            return Shape("ContentTypePartSettings_Edit", new { ContentPart = model }).Location("Content");
        }
    }

    public class BodyPartSettingsDisplayDriver : ContentTypePartDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
        {
            if (!String.Equals("BodyPart", model.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Shape("BodyPartSettings_Edit", new { ContentPart = model }).Location("Content");
        }
    }
}