using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Theming;
using Orchard.Localization;

namespace Orchard.DisplayManagement.Implementation
{
    public class DefaultIHtmlDisplay : IHtmlDisplay
    {
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IEnumerable<IShapeDisplayEvents> _shapeDisplayEvents;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IShapeBindingResolver> _shapeBindingResolvers;
        private readonly ILogger _logger;

        private readonly IThemeManager _themeManager;

        public DefaultIHtmlDisplay(
            IEnumerable<IShapeDisplayEvents> shapeDisplayEvents,
            IEnumerable<IShapeBindingResolver> shapeBindingResolvers,
            IHttpContextAccessor httpContextAccessor,
            IShapeTableManager shapeTableManager,
            ILogger<DefaultIHtmlDisplay> logger,
            IThemeManager themeManager)
        {
            _shapeTableManager = shapeTableManager;
            _shapeDisplayEvents = shapeDisplayEvents;
            _httpContextAccessor = httpContextAccessor;
            _shapeBindingResolvers = shapeBindingResolvers;
            _themeManager = themeManager;

            _logger = logger;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public async Task<IHtmlContent> ExecuteAsync(DisplayContext context)
        {
            var shape = context.Value as IShape;

            // non-shape arguments are returned as a no-op
            if (shape == null)
                return CoerceHtmlString(context.Value);

            var shapeMetadata = shape.Metadata;
            // can't really cope with a shape that has no type information
            if (shapeMetadata == null || string.IsNullOrEmpty(shapeMetadata.Type))
                return CoerceHtmlString(context.Value);

            var theme = await _themeManager.GetThemeAsync();
            var shapeTable = _shapeTableManager.GetShapeTable(theme?.Id);

            var displayingContext = new ShapeDisplayingContext
            {
                Shape = shape,
                ShapeMetadata = shapeMetadata,
                DisplayContext = context
            };

            // Use the same prefix as the shape
            context.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = shapeMetadata.Prefix;

            // Evaluate global Shape Display Events
            _shapeDisplayEvents.Invoke(sde => sde.Displaying(displayingContext), _logger);

            // Find base shape association using only the fundamental shape type.
            // Alternates that may already be registered do not affect the "displaying" event calls.
            ShapeBinding shapeBinding;
            if (TryGetDescriptorBinding(shapeMetadata.Type, Enumerable.Empty<string>(), shapeTable, out shapeBinding))
            {
                shapeBinding.ShapeDescriptor.Displaying.Invoke(action => action(displayingContext), _logger);

                // copy all binding sources (all templates for this shape) in order to use them as Localization scopes
                shapeMetadata.BindingSources = shapeBinding.ShapeDescriptor.BindingSources.Where(x => x != null).ToList();
                if (!shapeMetadata.BindingSources.Any())
                {
                    shapeMetadata.BindingSources.Add(shapeBinding.ShapeDescriptor.BindingSource);
                }
            }

            // invoking ShapeMetadata displaying events
            shapeMetadata.Displaying.Invoke(action => action(displayingContext), _logger);

            // use pre-fectched content if available (e.g. coming from specific cache implementation)
            if (displayingContext.ChildContent != null)
            {
                shape.Metadata.ChildContent = displayingContext.ChildContent;
            }
            else
            {
                // now find the actual binding to render, taking alternates into account
                ShapeBinding actualBinding;
                if (TryGetDescriptorBinding(shapeMetadata.Type, shapeMetadata.Alternates, shapeTable, out actualBinding))
                {
                    shapeBinding.ShapeDescriptor.Processing.Invoke(action => action(displayingContext), _logger);

                    // invoking ShapeMetadata processing events
                    await shapeMetadata.Processing.InvokeAsync(processing => processing(shape), _logger);

                    shape.Metadata.ChildContent = await ProcessAsync(actualBinding, shape, context);
                }
                else
                {
                    throw new OrchardException(T("Shape type {0} not found", shapeMetadata.Type));
                }
            }

            foreach (var frameType in shape.Metadata.Wrappers)
            {
                ShapeBinding frameBinding;
                if (TryGetDescriptorBinding(frameType, Enumerable.Empty<string>(), shapeTable, out frameBinding))
                {
                    shape.Metadata.ChildContent = await ProcessAsync(frameBinding, shape, context);
                }
            }

            var displayedContext = new ShapeDisplayedContext
            {
                Shape = shape,
                ShapeMetadata = shape.Metadata,
                ChildContent = shape.Metadata.ChildContent,
                DisplayContext = context
            };

            _shapeDisplayEvents.Invoke(sde =>
            {
                var prior = displayedContext.ChildContent = displayedContext.ShapeMetadata.ChildContent;
                sde.Displayed(displayedContext);
                // update the child content if the context variable has been reassigned
                if (prior != displayedContext.ChildContent)
                    displayedContext.ShapeMetadata.ChildContent = displayedContext.ChildContent;
            }, _logger);

            if (shapeBinding != null)
            {
                shapeBinding.ShapeDescriptor.Displayed.Invoke(action =>
                {
                    var prior = displayedContext.ChildContent = displayedContext.ShapeMetadata.ChildContent;
                    action(displayedContext);
                    // update the child content if the context variable has been reassigned
                    if (prior != displayedContext.ChildContent)
                        displayedContext.ShapeMetadata.ChildContent = displayedContext.ChildContent;
                }, _logger);
            }

            // invoking ShapeMetadata displayed events
            shapeMetadata.Displayed.Invoke(action => action(displayedContext), _logger);

            return shape.Metadata.ChildContent;
        }

        private bool TryGetDescriptorBinding(string shapeType, IEnumerable<string> shapeAlternates, ShapeTable shapeTable, out ShapeBinding shapeBinding)
        {
            // shape alternates are optional, fully qualified binding names
            // the earliest added alternates have the lowest priority
            // the descriptor returned is based on the binding that is matched, so it may be an entirely
            // different descriptor if the alternate has a different base name
            foreach (var shapeAlternate in shapeAlternates.Reverse())
            {
                foreach (var shapeBindingResolver in _shapeBindingResolvers)
                {
                    if (shapeBindingResolver.TryGetDescriptorBinding(shapeAlternate, out shapeBinding))
                    {
                        return true;
                    }
                }

                if (shapeTable.Bindings.TryGetValue(shapeAlternate, out shapeBinding))
                {
                    return true;
                }
            }

            // when no alternates match, the shapeType is used to find the longest matching binding
            // the shapetype name can break itself into shorter fallbacks at double-underscore marks
            // so the shapetype itself may contain a longer alternate forms that falls back to a shorter one
            var shapeTypeScan = shapeType;
            for (;;)
            {
                foreach (var shapeBindingResolver in _shapeBindingResolvers)
                {
                    if (shapeBindingResolver.TryGetDescriptorBinding(shapeTypeScan, out shapeBinding))
                    {
                        return true;
                    }
                }

                if (shapeTable.Bindings.TryGetValue(shapeTypeScan, out shapeBinding))
                {
                    return true;
                }

                var delimiterIndex = shapeTypeScan.LastIndexOf("__");
                if (delimiterIndex < 0)
                {
                    shapeBinding = null;
                    return false;
                }

                shapeTypeScan = shapeTypeScan.Substring(0, delimiterIndex);
            }
        }

        static IHtmlContent CoerceHtmlString(object value)
        {
            if (value == null)
                return null;

            var result = value as IHtmlContent;
            if (result != null)
                return result;

            return new HtmlString(HtmlEncoder.Default.Encode(value.ToString()));
        }

        static async Task<IHtmlContent> ProcessAsync(ShapeBinding shapeBinding, IShape shape, DisplayContext context)
        {
            if (shapeBinding == null || shapeBinding.BindingAsync == null)
            {
                // todo: create result from all child shapes
                return shape.Metadata.ChildContent ?? HtmlString.Empty;
            }
            return CoerceHtmlString(await shapeBinding.BindingAsync(context));
        }
    }
}