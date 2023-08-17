using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
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
        private readonly IContentPartDisplayDriverResolver _contentPartDisplayDriverResolver;
        private readonly IContentFieldDisplayDriverResolver _contentFieldDisplayDriverResolver;
        private readonly IEnumerable<IContentDisplayDriver> _displayDrivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly ILogger _logger;

        public ContentItemDisplayCoordinator(
            IContentPartDisplayDriverResolver contentPartDisplayDriverResolver,
            IContentFieldDisplayDriverResolver contentFieldDisplayDriverResolver,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentDisplayDriver> displayDrivers,
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            ILogger<ContentItemDisplayCoordinator> logger)
        {
            _contentPartDisplayDriverResolver = contentPartDisplayDriverResolver;
            _contentFieldDisplayDriverResolver = contentFieldDisplayDriverResolver;
            _contentPartFactory = contentPartFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _displayDrivers = displayDrivers;
            _logger = logger;
        }

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
                    InvokeExtensions.HandleException(ex, _logger, displayDriver.GetType().Name, nameof(BuildDisplayAsync));
                }
            }

            var hasStereotype = contentTypeDefinition.TryGetStereotype(out var stereotype);

            foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = contentTypePartDefinition.Name;
                var partTypeName = contentTypePartDefinition.PartDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partTypeName);
                var part = contentItem.Get(partActivator.Type, partName) as ContentPart;

                if (part == null)
                {
                    continue;
                }

                var contentType = contentTypePartDefinition.ContentTypeDefinition.Name;
                var partDisplayDrivers = _contentPartDisplayDriverResolver.GetDisplayModeDrivers(partTypeName, contentTypePartDefinition.DisplayMode());
                foreach (var partDisplayDriver in partDisplayDrivers)
                {
                    try
                    {
                        var result = await partDisplayDriver.BuildDisplayAsync(part, contentTypePartDefinition, context);
                        if (result != null)
                        {
                            await result.ApplyAsync(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        InvokeExtensions.HandleException(ex, _logger, partDisplayDrivers.GetType().Name, nameof(BuildDisplayAsync));
                    }
                }
                var tempContext = context;

                // Create a custom ContentPart shape that will hold the fields for dynamic content part (not implicit parts)
                // This allows its fields to be grouped and templated

                if (part.GetType() == typeof(ContentPart) && partTypeName != contentTypePartDefinition.ContentTypeDefinition.Name)
                {
                    var shapeType = context.DisplayType != "Detail" ? "ContentPart_" + context.DisplayType : "ContentPart";

                    var shapeResult = new ShapeResult(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType, () => new ValueTask<IShape>(new ZoneHolding(() => ctx.ShapeFactory.CreateAsync("Zone")))));
                    shapeResult.Differentiator(partName);
                    shapeResult.Name(partName);
                    shapeResult.Location("Content");
                    shapeResult.OnGroup(context.GroupId);
                    shapeResult.Displaying(ctx =>
                    {
                        var displayTypes = new[] { String.Empty, "_" + ctx.Shape.Metadata.DisplayType };

                        foreach (var displayType in displayTypes)
                        {
                            // eg. ServicePart,  ServicePart.Summary
                            ctx.Shape.Metadata.Alternates.Add($"{partTypeName}{displayType}");

                            // [ContentType]_[DisplayType]__[PartType]
                            // e.g. LandingPage-ServicePart, LandingPage-ServicePart.Summary
                            ctx.Shape.Metadata.Alternates.Add($"{contentType}{displayType}__{partTypeName}");

                            if (hasStereotype)
                            {
                                // [Stereotype]_[DisplayType]__[PartType],
                                // e.g. Widget-ServicePart
                                ctx.Shape.Metadata.Alternates.Add($"{stereotype}{displayType}__{partTypeName}");
                            }
                        }

                        if (partTypeName == partName)
                        {
                            return;
                        }

                        foreach (var displayType in displayTypes)
                        {
                            // [ContentType]_[DisplayType]__[PartName]
                            // e.g. Employee-Address1, Employee-Address2
                            ctx.Shape.Metadata.Alternates.Add($"{contentType}{displayType}__{partName}");

                            if (hasStereotype)
                            {
                                // [Stereotype]_[DisplayType]__[PartType]__[PartName]
                                // e.g. Widget-Services
                                ctx.Shape.Metadata.Alternates.Add($"{stereotype}{displayType}__{partTypeName}__{partName}");
                            }
                        }
                    });

                    await shapeResult.ApplyAsync(context);

                    var contentPartShape = shapeResult.Shape;

                    // Make the ContentPart name property available on the shape
                    contentPartShape.Properties[partTypeName] = part.Content;
                    contentPartShape.Properties["ContentItem"] = part.ContentItem;

                    context = new BuildDisplayContext(shapeResult.Shape, context.DisplayType, context.GroupId, context.ShapeFactory, context.Layout, context.Updater)
                    {
                        // With a new display context we have the default FindPlacementDelegate that returns null, so we reuse the delegate from the temp context.
                        FindPlacement = tempContext.FindPlacement,
                    };
                }

                foreach (var contentPartFieldDefinition in contentTypePartDefinition.PartDefinition.Fields)
                {
                    var fieldDisplayDrivers = _contentFieldDisplayDriverResolver.GetDisplayModeDrivers(contentPartFieldDefinition.FieldDefinition.Name, contentPartFieldDefinition.DisplayMode());
                    foreach (var fieldDisplayDriver in fieldDisplayDrivers)
                    {
                        try
                        {
                            var result = await fieldDisplayDriver.BuildDisplayAsync(part, contentPartFieldDefinition, contentTypePartDefinition, context);
                            if (result != null)
                            {
                                await result.ApplyAsync(context);
                            }
                        }
                        catch (Exception ex)
                        {
                            InvokeExtensions.HandleException(ex, _logger, fieldDisplayDriver.GetType().Name, nameof(BuildDisplayAsync));
                        }
                    }
                }

                context = tempContext;
            }
        }

        public async Task BuildEditorAsync(ContentItem contentItem, BuildEditorContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null)
            {
                return;
            }

            var contentShape = context.Shape as IZoneHolding;
            var partsShape = await context.ShapeFactory.CreateAsync("ContentZone",
                Arguments.From(new
                {
                    Identifier = contentItem.ContentItemId
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
                    InvokeExtensions.HandleException(ex, _logger, displayDriver.GetType().Name, nameof(BuildEditorAsync));
                }
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partTypeName = typePartDefinition.PartDefinition.Name;
                var partName = typePartDefinition.Name;
                var contentType = typePartDefinition.ContentTypeDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partTypeName);
                var part = (ContentPart)contentItem.Get(activator.Type, partName) ?? activator.CreateInstance();
                var partPosition = typePartDefinition.GetSettings<ContentTypePartSettings>().Position ?? "before";
                part.ContentItem = contentItem;

                // Create a custom shape to render all the part shapes into it
                var typePartShapeResult = CreateShapeResult(context.GroupId, partTypeName, contentType, typePartDefinition, partPosition);

                await typePartShapeResult.ApplyAsync(context);
                var typePartShape = typePartShapeResult.Shape;

                if (typePartShape == null)
                {
                    // Part is explicitly noop in placement then stop rendering execution
                    continue;
                }

                typePartShape.Properties["ContentPart"] = part;
                typePartShape.Properties["ContentTypePartDefinition"] = typePartDefinition;
                partsShape.Properties[partName] = typePartShape;

                context.DefaultZone = $"Parts.{partName}";
                context.DefaultPosition = partPosition;

                var partDisplayDrivers = _contentPartDisplayDriverResolver.GetEditorDrivers(partTypeName, typePartDefinition.Editor());
                await partDisplayDrivers.InvokeAsync(async (driver, part, typePartDefinition, context) =>
                {
                    var result = await driver.BuildEditorAsync(part, typePartDefinition, context);
                    if (result != null)
                    {
                        await result.ApplyAsync(context);
                    }
                }, part, typePartDefinition, context, _logger);

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;
                    var fieldPosition = partFieldDefinition.GetSettings<ContentPartFieldSettings>().Position ?? "before";

                    context.DefaultZone = $"Parts.{partName}:{fieldPosition}";
                    var fieldDisplayDrivers = _contentFieldDisplayDriverResolver.GetEditorDrivers(partFieldDefinition.FieldDefinition.Name, partFieldDefinition.Editor());
                    await fieldDisplayDrivers.InvokeAsync(async (driver, part, partFieldDefinition, typePartDefinition, context) =>
                    {
                        var result = await driver.BuildEditorAsync(part, partFieldDefinition, typePartDefinition, context);
                        if (result != null)
                        {
                            await result.ApplyAsync(context);
                        }
                    }, part, partFieldDefinition, typePartDefinition, context, _logger);
                }
            }
        }

        public async Task UpdateEditorAsync(ContentItem contentItem, UpdateEditorContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.LoadTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            var contentShape = context.Shape as IZoneHolding;
            var partsShape = await context.ShapeFactory.CreateAsync("ContentZone",
                Arguments.From(new
                {
                    Identifier = contentItem.ContentItemId
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
                    InvokeExtensions.HandleException(ex, _logger, displayDriver.GetType().Name, nameof(UpdateEditorAsync));
                }
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partTypeName = typePartDefinition.PartDefinition.Name;
                var partName = typePartDefinition.Name;
                var contentType = typePartDefinition.ContentTypeDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partTypeName);
                var part = (ContentPart)contentItem.Get(activator.Type, partName) ?? activator.CreateInstance();
                var partPosition = typePartDefinition.GetSettings<ContentTypePartSettings>().Position ?? "before";
                part.ContentItem = contentItem;

                // Create a custom shape to render all the part shapes into it
                var typePartShapeResult = CreateShapeResult(context.GroupId, partTypeName, contentType, typePartDefinition, partPosition);

                await typePartShapeResult.ApplyAsync(context);
                var typePartShape = typePartShapeResult.Shape;

                if (typePartShape == null)
                {
                    // Part is explicitly hidden in placement then stop rendering it
                    continue;
                }

                typePartShape.Properties["ContentPart"] = part;
                typePartShape.Properties["ContentTypePartDefinition"] = typePartDefinition;
                partsShape.Properties[partName] = typePartShape;

                context.DefaultZone = $"Parts.{partName}:{partPosition}";
                var partDisplayDrivers = _contentPartDisplayDriverResolver.GetEditorDrivers(partTypeName, typePartDefinition.Editor());
                await partDisplayDrivers.InvokeAsync(async (driver, part, typePartDefinition, context) =>
                {
                    var result = await driver.UpdateEditorAsync(part, typePartDefinition, context);
                    if (result != null)
                    {
                        await result.ApplyAsync(context);
                    }
                }, part, typePartDefinition, context, _logger);

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;
                    var fieldPosition = partFieldDefinition.GetSettings<ContentPartFieldSettings>().Position ?? "before";

                    context.DefaultZone = $"Parts.{partName}:{fieldPosition}";
                    var fieldDisplayDrivers = _contentFieldDisplayDriverResolver.GetEditorDrivers(partFieldDefinition.FieldDefinition.Name, partFieldDefinition.Editor());
                    await fieldDisplayDrivers.InvokeAsync(async (driver, part, partFieldDefinition, typePartDefinition, context) =>
                    {
                        var result = await driver.UpdateEditorAsync(part, partFieldDefinition, typePartDefinition, context);
                        if (result != null)
                        {
                            await result.ApplyAsync(context);
                        }
                    }, part, partFieldDefinition, typePartDefinition, context, _logger);
                }
            }
        }

        private static ShapeResult CreateShapeResult(string groupId, string partTypeName, string contentType, ContentTypePartDefinition typePartDefinition, string partPosition)
        {
            var shapeType = "ContentPart_Edit";
            var partName = typePartDefinition.Name;

            var typePartShapeResult = new ShapeResult(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType));
            typePartShapeResult.Differentiator($"{contentType}-{partName}");
            typePartShapeResult.Name(partName);
            typePartShapeResult.Location($"Parts:{partPosition}");
            typePartShapeResult.OnGroup(groupId);
            typePartShapeResult.Displaying(ctx =>
            {
                // ContentPart_Edit__[PartType]
                // eg ContentPart-ServicePart.Edit
                ctx.Shape.Metadata.Alternates.Add($"{shapeType}__{partTypeName}");

                // ContentPart_Edit__[ContentType]__[PartType]
                // e.g. ContentPart-LandingPage-ServicePart.Edit
                ctx.Shape.Metadata.Alternates.Add($"{shapeType}__{contentType}__{partTypeName}");

                var isNamedPart = typePartDefinition.PartDefinition.IsReusable() && partName != partTypeName;

                if (isNamedPart)
                {
                    // ContentPart_Edit__[ContentType]__[PartName]
                    // e.g. ContentPart-LandingPage-BillingService.Edit ContentPart-LandingPage-HelplineService.Edit
                    ctx.Shape.Metadata.Alternates.Add($"{shapeType}__{contentType}__{partName}");
                }
            });

            return typePartShapeResult;
        }
    }
}
