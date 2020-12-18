using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Implementation
{
    public class DefaultHtmlDisplay : IHtmlDisplay
    {
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IEnumerable<IShapeDisplayEvents> _shapeDisplayEvents;
        private readonly IEnumerable<IShapeBindingResolver> _shapeBindingResolvers;
        private readonly IThemeManager _themeManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public DefaultHtmlDisplay(
            IEnumerable<IShapeDisplayEvents> shapeDisplayEvents,
            IEnumerable<IShapeBindingResolver> shapeBindingResolvers,
            IShapeTableManager shapeTableManager,
            IServiceProvider serviceProvider,
            ILogger<DefaultHtmlDisplay> logger,
            IThemeManager themeManager)
        {
            _shapeTableManager = shapeTableManager;
            _shapeDisplayEvents = shapeDisplayEvents;
            _shapeBindingResolvers = shapeBindingResolvers;
            _themeManager = themeManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<IHtmlContent> ExecuteAsync(DisplayContext context)
        {
            var shape = context.Value as IShape;

            // non-shape arguments are returned as a no-op
            if (shape == null)
            {
                return CoerceHtmlString(context.Value);
            }

            var shapeMetadata = shape.Metadata;

            // can't really cope with a shape that has no type information
            if (shapeMetadata == null || string.IsNullOrEmpty(shapeMetadata.Type))
            {
                return CoerceHtmlString(context.Value);
            }

            // Copy the current context such that the rendering can customize it if necessary
            // For instance to change the HtmlFieldPrefix
            var localContext = new DisplayContext(context);
            localContext.HtmlFieldPrefix = shapeMetadata.Prefix ?? "";

            var displayContext = new ShapeDisplayContext
            {
                Shape = shape,
                DisplayContext = localContext,
                ServiceProvider = _serviceProvider
            };

            try
            {
                var theme = await _themeManager.GetThemeAsync();
                var shapeTable = _shapeTableManager.GetShapeTable(theme?.Id);

                // Evaluate global Shape Display Events
                await _shapeDisplayEvents.InvokeAsync((e, displayContext) => e.DisplayingAsync(displayContext), displayContext, _logger);

                // Find base shape association using only the fundamental shape type.
                // Alternates that may already be registered do not affect the "displaying" event calls.
                var shapeDescriptor = GetShapeDescriptor(shapeMetadata.Type, shapeTable);
                if (shapeDescriptor != null)
                {
                    await shapeDescriptor.DisplayingAsync.InvokeAsync((action, displayContext) => action(displayContext), displayContext, _logger);

                    // copy all binding sources (all templates for this shape) in order to use them as Localization scopes
                    shapeMetadata.BindingSources = shapeDescriptor.BindingSources.Where(x => x != null).ToList();
                    if (!shapeMetadata.BindingSources.Any())
                    {
                        shapeMetadata.BindingSources.Add(shapeDescriptor.BindingSource);
                    }
                }

                // invoking ShapeMetadata displaying events
                shapeMetadata.Displaying.Invoke(action => action(displayContext), _logger);

                // use pre-fetched content if available (e.g. coming from specific cache implementation)
                if (displayContext.ChildContent != null)
                {
                    shape.Metadata.ChildContent = displayContext.ChildContent;
                }

                if (shape.Metadata.ChildContent == null)
                {
                    // There might be no shape binding for the main shape, and only for its alternates.
                    if (shapeDescriptor != null)
                    {
                        await shapeDescriptor.ProcessingAsync.InvokeAsync((action, displayContext) => action(displayContext), displayContext, _logger);
                    }

                    // now find the actual binding to render, taking alternates into account
                    var actualBinding = await GetShapeBindingAsync(shapeMetadata.Type, shapeMetadata.Alternates, shapeTable);
                    if (actualBinding != null)
                    {
                        await shapeMetadata.ProcessingAsync.InvokeAsync((action, displayContext) => action(displayContext.Shape), displayContext, _logger);

                        shape.Metadata.ChildContent = await ProcessAsync(actualBinding, shape, localContext);
                    }
                    else
                    {
                        throw new Exception($"Shape type '{shapeMetadata.Type}' not found");
                    }
                }

                // Process wrappers
                if (shape.Metadata.Wrappers.Count > 0)
                {
                    foreach (var frameType in shape.Metadata.Wrappers)
                    {
                        var frameBinding = await GetShapeBindingAsync(frameType, AlternatesCollection.Empty, shapeTable);
                        if (frameBinding != null)
                        {
                            shape.Metadata.ChildContent = await ProcessAsync(frameBinding, shape, localContext);
                        }
                    }

                    // Clear wrappers to prevent the child content from rendering them again
                    shape.Metadata.Wrappers.Clear();
                }

                await _shapeDisplayEvents.InvokeAsync(async (e, displayContext) =>
                {
                    var prior = displayContext.ChildContent = displayContext.Shape.Metadata.ChildContent;

                    await e.DisplayedAsync(displayContext);

                    // update the child content if the context variable has been reassigned
                    if (prior != displayContext.ChildContent)
                    {
                        displayContext.Shape.Metadata.ChildContent = displayContext.ChildContent;
                    }
                }, displayContext, _logger);

                if (shapeDescriptor != null)
                {
                    await shapeDescriptor.DisplayedAsync.InvokeAsync(async (action, displayContext) =>
                    {
                        var prior = displayContext.ChildContent = displayContext.Shape.Metadata.ChildContent;

                        await action(displayContext);

                        // update the child content if the context variable has been reassigned
                        if (prior != displayContext.ChildContent)
                        {
                            displayContext.Shape.Metadata.ChildContent = displayContext.ChildContent;
                        }
                    }, displayContext, _logger);
                }

                // invoking ShapeMetadata displayed events
                shapeMetadata.Displayed.Invoke((action, displayContext) => action(displayContext), displayContext, _logger);
            }
            finally
            {
                await _shapeDisplayEvents.InvokeAsync((e, displayContext) => e.DisplayingFinalizedAsync(displayContext), displayContext, _logger);
            }

            return shape.Metadata.ChildContent;
        }

        private static ShapeDescriptor GetShapeDescriptor(string shapeType, ShapeTable shapeTable)
        {
            // Note: The shape type of a descriptor is a fundamental shape type that never contains
            // any '__' separator. If a shape type contains some '__' separators, its fundamental
            // shape type is the left part just before the 1st occurrence of the '__' separator.

            // As a fast path we 1st use the shapeType as is but it may contain some '__'.
            if (!shapeTable.Descriptors.TryGetValue(shapeType, out var shapeDescriptor))
            {
                // Check if not a fundamental type.
                var index = shapeType.IndexOf("__", StringComparison.Ordinal);

                if (index > 0)
                {
                    // Try again by using the fundamental shape type without any '__' separator.
                    shapeTable.Descriptors.TryGetValue(shapeType.Substring(0, index), out shapeDescriptor);
                }
            }

            return shapeDescriptor;
        }

        private async Task<ShapeBinding> GetShapeBindingAsync(string shapeType, AlternatesCollection shapeAlternates, ShapeTable shapeTable)
        {
            // shape alternates are optional, fully qualified binding names
            // the earliest added alternates have the lowest priority
            // the descriptor returned is based on the binding that is matched, so it may be an entirely
            // different descriptor if the alternate has a different base name
            for (var i = shapeAlternates.Count - 1; i >= 0; i--)
            {
                var shapeAlternate = shapeAlternates[i];

                foreach (var shapeBindingResolver in _shapeBindingResolvers)
                {
                    var binding = await shapeBindingResolver.GetShapeBindingAsync(shapeAlternate);

                    if (binding != null)
                    {
                        return binding;
                    }
                }

                if (shapeTable.Bindings.TryGetValue(shapeAlternate, out var shapeBinding))
                {
                    return shapeBinding;
                }
            }

            // when no alternates match, the shapeType is used to find the longest matching binding
            // the shapetype name can break itself into shorter fallbacks at double-underscore marks
            // so the shapetype itself may contain a longer alternate forms that falls back to a shorter one
            var shapeTypeScan = shapeType;

            do
            {
                foreach (var shapeBindingResolver in _shapeBindingResolvers)
                {
                    var binding = await shapeBindingResolver.GetShapeBindingAsync(shapeTypeScan);

                    if (binding != null)
                    {
                        return binding;
                    }
                }

                if (shapeTable.Bindings.TryGetValue(shapeTypeScan, out var shapeBinding))
                {
                    return shapeBinding;
                }
            }
            while (TryGetParentShapeTypeName(ref shapeTypeScan));

            return null;
        }

        private static IHtmlContent CoerceHtmlString(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is IHtmlContent result)
            {
                return result;

                // To prevent the result from being rendered lately, we can
                // serialize it right away. But performance seems to be better
                // like this, until we find this is an issue.

                //using (var html = new StringWriter())
                //{
                //    result.WriteTo(html, htmlEncoder);
                //    return new HtmlString(html.ToString());
                //}
            }

            // Convert to a string and HTML-encode it
            return new StringHtmlContent(value.ToString());
        }

        private static bool TryGetParentShapeTypeName(ref string shapeTypeScan)
        {
            var delimiterIndex = shapeTypeScan.LastIndexOf("__", StringComparison.Ordinal);
            if (delimiterIndex > 0)
            {
                shapeTypeScan = shapeTypeScan.Substring(0, delimiterIndex);
                return true;
            }
            return false;
        }

        private static ValueTask<IHtmlContent> ProcessAsync(ShapeBinding shapeBinding, IShape shape, DisplayContext context)
        {
            async ValueTask<IHtmlContent> Awaited(Task<IHtmlContent> task)
            {
                return CoerceHtmlString(await task);
            }

            if (shapeBinding?.BindingAsync == null)
            {
                // todo: create result from all child shapes
                return new ValueTask<IHtmlContent>(shape.Metadata.ChildContent ?? HtmlString.Empty);
            }

            var task = shapeBinding.BindingAsync(context);

            if (!task.IsCompletedSuccessfully)
            {
                return Awaited(task);
            }

            return new ValueTask<IHtmlContent>(CoerceHtmlString(task.Result));
        }
    }
}
