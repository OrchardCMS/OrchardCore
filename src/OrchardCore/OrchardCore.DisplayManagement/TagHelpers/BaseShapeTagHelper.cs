using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Mvc.Utilities;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    public abstract class BaseShapeTagHelper : TagHelper
    {
        private static readonly HashSet<string> InternalProperties = new HashSet<string>
        {
            "id", "alternate", "wrapper", "cache-id", "cache-context", "cache-tag", "cache-fixed-duration", "cache-sliding-duration"
        };

        protected const string SlotName = "slot";
        protected const string PropertyDictionaryName = "prop-all";
        protected const string PropertyPrefix = "prop-";
        private static readonly char[] Separators = { ',', ' ' };

        protected IShapeFactory _shapeFactory;
        protected IDisplayHelper _displayHelper;
        protected IShapeScopeManager _shapeScopeManager;

        public string Type { get; set; }

        // The following properties are declared as internal to prevent any attribute with a
        // matching name from being automatically bound and removed from the output attributes,
        // and then not added to the properties of the shape we are building.

        internal string Cache { get; set; }
        internal TimeSpan? FixedDuration { get; set; }
        internal TimeSpan? SlidingDuration { get; set; }
        internal string Context { get; set; }
        internal string Tag { get; set; }

        protected BaseShapeTagHelper(
            IShapeScopeManager shapeScopeManager,
            IShapeFactory shapeFactory,
            IDisplayHelper displayHelper)
        {
            _shapeFactory = shapeFactory;
            _displayHelper = displayHelper;
            _shapeScopeManager = shapeScopeManager;
        }

        [HtmlAttributeName("Shape")]
        public dynamic Shape { get; set; }

        [HtmlAttributeName(SlotName)]
        public string Slot { get; set; }

        /// <summary>
        /// Additional properties for the shape.
        /// </summary>
        [HtmlAttributeName(PropertyDictionaryName, DictionaryAttributePrefix = PropertyPrefix)]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public override async Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            var properties = new Dictionary<string, object>();

            // These prefixed properties are bound with their original type and not converted as IHtmlContent
            foreach (var property in Properties)
            {
                var normalizedName = property.Key.ToPascalCaseDash();
                properties.Add(normalizedName, property.Value);
            }

            // Extract all other attributes from the tag helper, which are passed as IHtmlContent
            foreach (var pair in output.Attributes)
            {
                // Check it's not a reserved property name
                if (!InternalProperties.Contains(pair.Name))
                {
                    var normalizedName = pair.Name.ToPascalCaseDash();

                    if (!properties.ContainsKey(normalizedName))
                    {
                        properties.Add(normalizedName, pair.Value.ToString());
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(Type))
            {
                Type = output.TagName;
            }

            if (string.IsNullOrWhiteSpace(Cache) && output.Attributes.ContainsName("cache-id"))
            {
                Cache = Convert.ToString(output.Attributes["cache-id"].Value);
            }

            if (string.IsNullOrWhiteSpace(Context) && output.Attributes.ContainsName("cache-context"))
            {
                Context = Convert.ToString(output.Attributes["cache-context"].Value);
            }

            if (string.IsNullOrWhiteSpace(Tag) && output.Attributes.ContainsName("cache-tag"))
            {
                Tag = Convert.ToString(output.Attributes["cache-tag"].Value);
            }

            if (!FixedDuration.HasValue && output.Attributes.ContainsName("cache-fixed-duration"))
            {
                TimeSpan timespan;
                if (TimeSpan.TryParse(Convert.ToString(output.Attributes["cache-fixed-duration"].Value), out timespan))
                {
                    FixedDuration = timespan;
                }
            }

            if (!SlidingDuration.HasValue && output.Attributes.ContainsName("cache-sliding-duration"))
            {
                TimeSpan timespan;
                if (TimeSpan.TryParse(Convert.ToString(output.Attributes["cache-sliding-duration"].Value), out timespan))
                {
                    SlidingDuration = timespan;
                }
            }

            if (Shape == null)
            {
                Shape = await _shapeFactory.CreateAsync(Type, Arguments.From(properties));
            }

            if (output.Attributes.ContainsName("id"))
            {
                Shape.Id = Convert.ToString(output.Attributes["id"].Value);
            }

            if (output.Attributes.ContainsName("alternate"))
            {
                Shape.Metadata.Alternates.Add(Convert.ToString(output.Attributes["alternate"].Value));
            }

            if (output.Attributes.ContainsName("wrapper"))
            {
                Shape.Metadata.Wrappers.Add(Convert.ToString(output.Attributes["wrapper"].Value));
            }


                // TODO move these into scope so shapes can be nested properly.
                // tagHelperContext.Items.Add(typeof(IShape), Shape);

            if (!string.IsNullOrWhiteSpace(Cache))
            {
                ShapeMetadata metadata = Shape.Metadata;

                metadata.Cache(Cache);

                if (FixedDuration.HasValue)
                {
                    metadata.Cache().WithExpiryAfter(FixedDuration.Value);
                }

                if (SlidingDuration.HasValue)
                {
                    metadata.Cache().WithExpirySliding(SlidingDuration.Value);
                }

                if (!string.IsNullOrWhiteSpace(Context))
                {
                    var contexts = Context.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddContext(contexts);
                }

                if (!string.IsNullOrWhiteSpace(Tag))
                {
                    var tags = Tag.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddTag(tags);
                }
            }

            _shapeScopeManager.EnterScope(new ShapeScopeContext());

            if (Shape != null)
            {
                if (Shape is IShape shape)
                {
                    _shapeScopeManager.AddShape(shape);
                }

            }

            // Always render the child content, but we might not use it.
            // This allows the tag helpers in the child content to activate.

            // What happens to this child content?
            await output.GetChildContentAsync();

            var content = await _displayHelper.ShapeExecuteAsync(Shape);

            if (!String.IsNullOrEmpty(Slot))
            {
                _shapeScopeManager.AddSlot(Slot, content);
            }
            else
            {
                // Hmm does this child content not go into the slot, if it has a slot name.
                output.Content.SetHtmlContent(content);
            }


            _shapeScopeManager.ExitScope();

            // We don't want any encapsulating tag around the shape
            output.TagName = null;
        }
    }
}
