using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display
{
    /// <summary>
    /// Provides a concrete implementation of a display handler coordinating part, field and content item drivers.
    /// </summary>
    public class ContentItemDisplayCoordinator : IContentDisplayHandler
    {
        private readonly IEnumerable<IContentDisplayDriver> _displayDrivers;
        private readonly IEnumerable<IContentFieldDisplayDriver> _fieldDisplayDrivers;
        private readonly IEnumerable<IContentPartDisplayDriver> _partDisplayDrivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentPartFactory _contentPartFactory;

        public ContentItemDisplayCoordinator(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentDisplayDriver> displayDrivers,
            IEnumerable<IContentFieldDisplayDriver> fieldDisplayDrivers,
            IEnumerable<IContentPartDisplayDriver> partDisplayDrivers,
            IContentPartFactory contentPartFactory,
            ILogger<ContentItemDisplayCoordinator> logger)
        {
            _contentPartFactory = contentPartFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _displayDrivers = displayDrivers;
            _fieldDisplayDrivers = fieldDisplayDrivers;
            _partDisplayDrivers = partDisplayDrivers;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

        public async Task BuildDisplayAsync(ContentItem contentItem, BuildDisplayContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if(contentTypeDefinition == null)
            {
                return;
            }

            foreach (var displayDriver in _displayDrivers)
            {
                try
                {
                    var result = await displayDriver.BuildDisplayAsync(contentItem, context);
                    if (result != null)
                    {
                        result.Apply(context);
                    }
                }
                catch (Exception ex)
                {
                    InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, "BuildDisplayAsync");
                }
            }

            foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = contentTypePartDefinition.Name;
                var partTypeName = contentTypePartDefinition.PartDefinition.Name;
                var partType = _contentPartFactory.GetContentPartType(partTypeName) ?? typeof(ContentPart);
                var part = contentItem.Get(partType, partName) as ContentPart;

                foreach (var displayDriver in _partDisplayDrivers)
                {
                    try
                    {
                        if (part != null)
                        {
                            var result = await displayDriver.BuildDisplayAsync(part, contentTypePartDefinition, context);
                            if (result != null)
                            {
                                result.Apply(context);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, "BuildDisplayAsync");
                    }
                }

                foreach (var contentPartFieldDefinition in contentTypePartDefinition.PartDefinition.Fields)
                {
                    foreach (var displayDriver in _fieldDisplayDrivers)
                    {
                        try
                        {
                            var result = await displayDriver.BuildDisplayAsync(part, contentPartFieldDefinition, contentTypePartDefinition, context);
                            if (result != null)
                            {
                                result.Apply(context);
                            }
                        }
                        catch (Exception ex)
                        {
                            InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, "BuildDisplayAsync");
                        }
                    }
                }
            }
        }

        public async Task BuildEditorAsync(ContentItem contentItem, BuildEditorContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            dynamic contentShape = context.Shape;
            dynamic partsShape = context.ShapeFactory.Create("Zone", Arguments.Empty);
            contentShape.Zones["Parts"] = partsShape;

            foreach (var displayDriver in _displayDrivers)
            {
                try
                {
                    var result = await displayDriver.BuildEditorAsync(contentItem, context);
                    if (result != null)
                    {
                        result.Apply(context);
                    }
                }
                catch (Exception ex)
                {
                    InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, nameof(BuildEditorAsync));
                }
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                ContentPart part;

                part = _contentPartFactory.CreateContentPart(typePartDefinition.Name) ?? new ContentPart();
                part = (ContentPart)contentItem.Get(part.GetType(), typePartDefinition.Name) ?? part;
                part.ContentItem = contentItem;

                // Create a custom shape to render all the part shapes into it
                dynamic typePartShape = context.ShapeFactory.Create("ContentPart_Edit", Arguments.Empty);
                typePartShape.ContentPart = part;
                typePartShape.ContentTypePartDefinition = typePartDefinition;

                var partPosition = typePartDefinition.Settings["Position"]?.ToString() ?? "before";

                partsShape.Add(typePartShape, partPosition);
                partsShape[typePartDefinition.Name] = typePartShape;

                context.FindPlacement = (shapeType, differentiator, displayType, displayContext) => new PlacementInfo { Location = $"Parts.{typePartDefinition.Name}" };

                await _partDisplayDrivers.InvokeAsync(async contentDisplay =>
                {
                    var result = await contentDisplay.BuildEditorAsync(part, typePartDefinition, context);
                    if (result != null)
                    {
                        result.Apply(context);
                    }
                }, Logger);

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;

                    var fieldPosition = partFieldDefinition.Settings["Position"]?.ToString() ?? "before";
                    context.FindPlacement = (shapeType, differentiator, displayType, displayContext) => new PlacementInfo { Location = $"Parts.{typePartDefinition.Name}:{fieldPosition}" };

                    await _fieldDisplayDrivers.InvokeAsync(async contentDisplay =>
                    {
                        var result = await contentDisplay.BuildEditorAsync(part, partFieldDefinition, typePartDefinition, context);
                        if (result != null)
                        {
                            result.Apply(context);
                        }
                    }, Logger);
                }
            }
        }

        public async Task UpdateEditorAsync(ContentItem contentItem, UpdateEditorContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            dynamic contentShape = context.Shape;
            dynamic partsShape = context.ShapeFactory.Create("Zone", Arguments.Empty);
            contentShape.Zones["Parts"] = partsShape;

            foreach (var displayDriver in _displayDrivers)
            {
                try
                {
                    var result = await displayDriver.UpdateEditorAsync(contentItem, context);
                    if (result != null)
                    {
                        result.Apply(context);
                    }
                }
                catch (Exception ex)
                {
                    InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, nameof(UpdateEditorAsync));
                }
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                ContentPart part;

                part = _contentPartFactory.CreateContentPart(typePartDefinition.Name) ?? new ContentPart();
                part = (ContentPart)contentItem.Get(part.GetType(), typePartDefinition.Name) ?? part;
                part.ContentItem = contentItem;

                // Create a custom shape to render all the part shapes into it
                dynamic typePartShape = context.ShapeFactory.Create("ContentPart_Edit", Arguments.Empty);
                typePartShape.ContentPart = part;
                typePartShape.ContentTypePartDefinition = typePartDefinition;

                var partPosition = typePartDefinition.Settings["Position"]?.ToString() ?? "before";

                partsShape.Add(typePartShape, partPosition);
                partsShape[typePartDefinition.Name] = typePartShape;
                
                context.FindPlacement = (shapeType, differentiator, displayType, displayContext) => new PlacementInfo { Location = $"Parts.{typePartDefinition.Name}" };

                await _partDisplayDrivers.InvokeAsync(async contentDisplay =>
                {
                    var result = await contentDisplay.UpdateEditorAsync(part, typePartDefinition, context);
                    if (result != null)
                    {
                        result.Apply(context);
                    }
                }, Logger);

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;

                    var fieldPosition = partFieldDefinition.Settings["Position"]?.ToString() ?? "before";
                    context.FindPlacement = (shapeType, differentiator, displayType, displayContext) => new PlacementInfo { Location = $"Parts.{typePartDefinition.Name}:{fieldPosition}" };

                    await _fieldDisplayDrivers.InvokeAsync(async contentDisplay =>
                    {
                        var result = await contentDisplay.UpdateEditorAsync(part, partFieldDefinition, typePartDefinition, context);
                        if (result != null)
                        {
                            result.Apply(context);
                        }
                    }, Logger);
                }
            }
        }
    }
}
