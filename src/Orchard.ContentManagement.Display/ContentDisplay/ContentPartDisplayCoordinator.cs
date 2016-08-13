using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Zones;

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
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;

        public ContentItemDisplayCoordinator(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentDisplayDriver> displayDrivers,
            IEnumerable<IContentFieldDisplayDriver> fieldDisplayDrivers,
            IEnumerable<IContentPartDisplayDriver> partDisplayDrivers,
            IEnumerable<IContentPartDriver> partDrivers,
            ILogger<ContentItemDisplayCoordinator> logger)
        {
            _contentPartDrivers = partDrivers;
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

            var partInfos = _contentPartDrivers.Select(x => x.GetPartInfo()).ToDictionary(x => x.PartName);

            foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = contentTypePartDefinition.Name;
                var partTypeName = contentTypePartDefinition.PartDefinition.Name;
                var partType = partInfos.ContainsKey(partTypeName) ? partInfos[partTypeName].Factory(contentTypePartDefinition).GetType() : null;
                var part = contentItem.Get(partType ?? typeof(ContentPart), partName) as ContentPart;

                foreach (var displayDriver in _partDisplayDrivers)
                {
                    try
                    {
                        var result = await displayDriver.BuildDisplayAsync(part, contentTypePartDefinition, context);
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

            var partInfos = _contentPartDrivers.Select(x => x.GetPartInfo()).ToDictionary(x => x.PartName);

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                ContentPartInfo partInfo;
                ContentPart part;

                if (partInfos.TryGetValue(typePartDefinition.PartDefinition.Name, out partInfo))
                {
                    // It's a well-known part type, bind the data to the model
                    part = partInfo.Factory(typePartDefinition);
                }
                else
                {
                    // Generic content part model (custom part)
                    part = new ContentPart();
                }

                part = contentItem.GetOrCreate(part.GetType(), () => new ContentPart(), typePartDefinition.Name) as ContentPart;

                // Create a custom shape to render all the part shapes into it
                dynamic typePartShape = context.ShapeFactory.Create("ContentPart_Edit", Arguments.Empty);
                typePartShape.ContentPart = part;
                typePartShape.ContentTypePartDefinition = typePartDefinition;

                partsShape.Add(typePartShape); // TODO: Add location from settings
                partsShape[typePartDefinition.Name] = typePartShape;

                // TODO: Add location from settings
                context.FindPlacement = (shape, differentiator, displayType) => new PlacementInfo { Location = $"Parts.{typePartDefinition.Name}" };

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

            var partInfos = _contentPartDrivers.Select(x => x.GetPartInfo()).ToDictionary(x => x.PartName);

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                ContentPartInfo partInfo;
                ContentPart part;

                if (partInfos.TryGetValue(typePartDefinition.PartDefinition.Name, out partInfo))
                {
                    // It's a well-known part type, bind the data to the model
                    part = partInfo.Factory(typePartDefinition);
                }
                else
                {
                    // Generic content part model (custom part)
                    part = new ContentPart();
                }

                part = contentItem.GetOrCreate(part.GetType(), () => new ContentPart(), typePartDefinition.Name) as ContentPart;

                await _partDisplayDrivers.InvokeAsync(async contentDisplay =>
                {
                    var result = await contentDisplay.UpdateEditorAsync(part, typePartDefinition, context);
                    if (result != null)
                        result.Apply(context);
                }, Logger);

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;
                    await _fieldDisplayDrivers.InvokeAsync(async contentDisplay =>
                    {
                        var result = await contentDisplay.UpdateEditorAsync(part, partFieldDefinition, typePartDefinition, context);
                        if (result != null)
                            result.Apply(context);
                    }, Logger);
                }
            }
        }
    }
}
