using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Display
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
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;

        public ContentItemDisplayCoordinator(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentDisplayDriver> displayDrivers,
            IEnumerable<IContentFieldDisplayDriver> fieldDisplayDrivers,
            IEnumerable<IContentPartDisplayDriver> partDisplayDrivers,
            ITypeActivatorFactory<ContentPart> contentPartFactory,
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

            if (contentTypeDefinition == null)
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
                        await result.ApplyAsync(context);
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
                var contentType = contentTypePartDefinition.ContentTypeDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partTypeName);
                var part = contentItem.Get(partActivator.Type, partName) as ContentPart;

                if (part != null)
                {
                    foreach (var displayDriver in _partDisplayDrivers)
                    {
                        try
                        {
                            var result = await displayDriver.BuildDisplayAsync(part, contentTypePartDefinition, context);
                            if (result != null)
                            {
                                await result.ApplyAsync(context);
                            }
                        }
                        catch (Exception ex)
                        {
                            InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, "BuildDisplayAsync");
                        }
                    }

                    var tempContext = context;

                    // Create a custom ContentPart shape that will hold the fields for dynamic content part (not implicit parts)
                    // This allows its fields to be grouped and templated

                    if (part.GetType() == typeof(ContentPart) && partTypeName != contentTypePartDefinition.ContentTypeDefinition.Name)
                    {
                        var shapeType = context.DisplayType != "Detail" ? "ContentPart_" + context.DisplayType : "ContentPart";

                        var shapeResult = new ShapeResult(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType, () => new ValueTask<IShape>(new ZoneHolding(() => ctx.ShapeFactory.CreateAsync("Zone", Arguments.Empty)))));
                        shapeResult.Differentiator(partName);
                        shapeResult.Location("Content");

                        await shapeResult.ApplyAsync(context);

                        var contentPartShape = shapeResult.Shape;

                        // Make the ContentPart name property available on the shape
                        dynamic dynamicContentPartShape = contentPartShape;
                        dynamicContentPartShape[partTypeName] = part.Content;
                        dynamicContentPartShape["ContentItem"] = part.ContentItem;

                        contentPartShape.Metadata.Alternates.Add(partTypeName);
                        contentPartShape.Metadata.Alternates.Add($"{contentType}__{partTypeName}");

                        if (context.DisplayType != "Detail")
                        {
                            contentPartShape.Metadata.Alternates.Add($"{partTypeName}_{context.DisplayType}");
                            contentPartShape.Metadata.Alternates.Add($"{contentType}_{context.DisplayType}__{partTypeName}");
                        }

                        if (partName != partTypeName)
                        {
                            contentPartShape.Metadata.Alternates.Add($"{contentType}__{partName}");

                            if (context.DisplayType != "Detail")
                            {
                                contentPartShape.Metadata.Alternates.Add($"{contentType}_{context.DisplayType}__{partName}");
                            }
                        }

                        context = new BuildDisplayContext(shapeResult.Shape, context.DisplayType, context.GroupId, context.ShapeFactory, context.Layout, context.Updater);
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
                                    await result.ApplyAsync(context);
                                }
                            }
                            catch (Exception ex)
                            {
                                InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, "BuildDisplayAsync");
                            }
                        }
                    }

                    context = tempContext;
                }
            }
        }

        public async Task BuildEditorAsync(ContentItem contentItem, BuildEditorContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            dynamic contentShape = context.Shape;
            dynamic partsShape = await context.ShapeFactory.CreateAsync("ContentZone",
                Arguments.From(new
                {
                    ContentItem = contentItem
                }));

            contentShape.Zones["Parts"] = partsShape;

            foreach (var displayDriver in _displayDrivers)
            {
                try
                {
                    var result = await displayDriver.BuildEditorAsync(contentItem, context);
                    if (result != null)
                    {
                        await result.ApplyAsync(context);
                    }
                }
                catch (Exception ex)
                {
                    InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, nameof(BuildEditorAsync));
                }
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partTypeName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partTypeName);
                var part = (ContentPart)contentItem.Get(activator.Type, typePartDefinition.Name) ?? activator.CreateInstance();
                part.ContentItem = contentItem;

                // Create a custom shape to render all the part shapes into it
                dynamic typePartShape = await context.ShapeFactory.CreateAsync("ContentPart_Edit", Arguments.Empty);
                typePartShape.ContentPart = part;
                typePartShape.ContentTypePartDefinition = typePartDefinition;

                var partPosition = typePartDefinition.GetSettings<ContentTypePartSettings>().Position ?? "before";

                partsShape.Add(typePartShape, partPosition);
                partsShape[typePartDefinition.Name] = typePartShape;

                context.DefaultZone = $"Parts.{typePartDefinition.Name}";
                context.DefaultPosition = partPosition;

                await _partDisplayDrivers.InvokeAsync(async contentDisplay =>
                {
                    var result = await contentDisplay.BuildEditorAsync(part, typePartDefinition, context);
                    if (result != null)
                    {
                        await result.ApplyAsync(context);
                    }
                }, Logger);

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;
                    var fieldPosition = partFieldDefinition.GetSettings<ContentPartFieldSettings>().Position ?? "before";

                    context.DefaultZone = $"Parts.{typePartDefinition.Name}:{fieldPosition}";

                    await _fieldDisplayDrivers.InvokeAsync(async contentDisplay =>
                    {
                        var result = await contentDisplay.BuildEditorAsync(part, partFieldDefinition, typePartDefinition, context);
                        if (result != null)
                        {
                            await result.ApplyAsync(context);
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
            dynamic partsShape = await context.ShapeFactory.CreateAsync("ContentZone",
                Arguments.From(new
                {
                    ContentItem = contentItem
                }));

            contentShape.Zones["Parts"] = partsShape;

            foreach (var displayDriver in _displayDrivers)
            {
                try
                {
                    var result = await displayDriver.UpdateEditorAsync(contentItem, context);
                    if (result != null)
                    {
                        await result.ApplyAsync(context);
                    }
                }
                catch (Exception ex)
                {
                    InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, nameof(UpdateEditorAsync));
                }
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partTypeName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partTypeName);
                var part = (ContentPart)contentItem.Get(activator.Type, typePartDefinition.Name) ?? activator.CreateInstance();
                part.ContentItem = contentItem;

                // Create a custom shape to render all the part shapes into it
                dynamic typePartShape = await context.ShapeFactory.CreateAsync("ContentPart_Edit", Arguments.Empty);
                typePartShape.ContentPart = part;
                typePartShape.ContentTypePartDefinition = typePartDefinition;
                var partPosition = typePartDefinition.GetSettings<ContentTypePartSettings>().Position ?? "before";

                partsShape.Add(typePartShape, partPosition);
                partsShape[typePartDefinition.Name] = typePartShape;

                context.DefaultZone = $"Parts.{typePartDefinition.Name}:{partPosition}";

                await _partDisplayDrivers.InvokeAsync(async contentDisplay =>
                {
                    var result = await contentDisplay.UpdateEditorAsync(part, typePartDefinition, context);
                    if (result != null)
                    {
                        await result.ApplyAsync(context);
                    }
                }, Logger);

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;
                    var fieldPosition = partFieldDefinition.GetSettings<ContentPartFieldSettings>().Position ?? "before";

                    context.DefaultZone = $"Parts.{typePartDefinition.Name}:{fieldPosition}";

                    await _fieldDisplayDrivers.InvokeAsync(async contentDisplay =>
                    {
                        var result = await contentDisplay.UpdateEditorAsync(part, partFieldDefinition, typePartDefinition, context);
                        if (result != null)
                        {
                            await result.ApplyAsync(context);
                        }
                    }, Logger);
                }
            }
        }
    }
}
