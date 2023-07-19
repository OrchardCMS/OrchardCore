using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    public abstract class BaseShapeTagHelper : TagHelper
    {
        protected const string PropertyDictionaryName = "prop-all";
        protected const string PropertyPrefix = "prop-";

        private static readonly HashSet<string> _internalProperties = new()
        {
            "id", "alternate", "wrapper", "cache-id", "cache-context", "cache-tag", "cache-fixed-duration", "cache-sliding-duration"
        };

        private static readonly char[] _separators = { ',', ' ' };

        protected IShapeFactory _shapeFactory;
        protected IDisplayHelper _displayHelper;

        public string Type { get; set; }

        // The following properties are declared as internal to prevent any attribute with a
        // matching name from being automatically bound and removed from the output attributes,
        // and then not added to the properties of the shape we are building.

        internal string Cache { get; set; }
        internal TimeSpan? FixedDuration { get; set; }
        internal TimeSpan? SlidingDuration { get; set; }
        internal string Context { get; set; }
        internal string Tag { get; set; }

        protected BaseShapeTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
        {
            _shapeFactory = shapeFactory;
            _displayHelper = displayHelper;
        }

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
                if (!_internalProperties.Contains(pair.Name))
                {
                    var normalizedName = pair.Name.ToPascalCaseDash();

                    if (!properties.ContainsKey(normalizedName))
                    {
                        properties.Add(normalizedName, pair.Value.ToString());
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(Type))
            {
                Type = output.TagName;
            }

            if (String.IsNullOrWhiteSpace(Cache) && output.Attributes.ContainsName("cache-id"))
            {
                Cache = Convert.ToString(output.Attributes["cache-id"].Value);
            }

            if (String.IsNullOrWhiteSpace(Context) && output.Attributes.ContainsName("cache-context"))
            {
                Context = Convert.ToString(output.Attributes["cache-context"].Value);
            }

            if (String.IsNullOrWhiteSpace(Tag) && output.Attributes.ContainsName("cache-tag"))
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

            var shape = await _shapeFactory.CreateAsync(Type, Arguments.From(properties));

            if (output.Attributes.ContainsName("id"))
            {
                shape.Id = Convert.ToString(output.Attributes["id"].Value);
            }

            if (output.Attributes.ContainsName("alternate"))
            {
                shape.Metadata.Alternates.Add(Convert.ToString(output.Attributes["alternate"].Value));
            }

            if (output.Attributes.ContainsName("wrapper"))
            {
                shape.Metadata.Wrappers.Add(Convert.ToString(output.Attributes["wrapper"].Value));
            }

            tagHelperContext.Items.Add(typeof(IShape), shape);

            if (!String.IsNullOrWhiteSpace(Cache))
            {
                var metadata = shape.Metadata;

                metadata.Cache(Cache);

                if (FixedDuration.HasValue)
                {
                    metadata.Cache().WithExpiryAfter(FixedDuration.Value);
                }

                if (SlidingDuration.HasValue)
                {
                    metadata.Cache().WithExpirySliding(SlidingDuration.Value);
                }

                if (!String.IsNullOrWhiteSpace(Context))
                {
                    var contexts = Context.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddContext(contexts);
                }

                if (!String.IsNullOrWhiteSpace(Tag))
                {
                    var tags = Tag.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddTag(tags);
                }
            }

            await output.GetChildContentAsync();

            output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));

            // We don't want any encapsulating tag around the shape
            output.TagName = null;
        }
    }
}
