using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    public abstract class BaseComponentShapeTagHelper : TagHelper
    {
        private static readonly HashSet<string> InternalProperties = new HashSet<string>
        {
            "id", "alternate", "wrapper", "cache-id", "cache-context", "cache-tag", "cache-fixed-duration", "cache-sliding-duration"
        };

        protected const string PropertyDictionaryName = "prop-all";
        protected const string PropertyPrefix = "prop-";
        private static readonly char[] Separators = { ',', ' ' };

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

        public BaseComponentShapeTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
        {
            _shapeFactory = shapeFactory;
            _displayHelper = displayHelper;
        }

        [HtmlAttributeName("Slot")]
        public string Slot { get; set; }


        /// <summary>
        /// Additional properties for the shape.
        /// </summary>
        [HtmlAttributeName(PropertyDictionaryName, DictionaryAttributePrefix = PropertyPrefix)]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, object> NormalizedProperties { get; set; } = new Dictionary<string, object>();
        public abstract ValueTask<IShape> BuildComponentAsync();

        public override async Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
        {

            // These prefixed properties are bound with their original type and not converted as IHtmlContent
            foreach (var property in Properties)
            {
                var normalizedName = property.Key.ToPascalCaseDash();
                NormalizedProperties.Add(normalizedName, property.Value);
            }

            // Extract all other attributes from the tag helper, which are passed as IHtmlContent
            foreach (var pair in output.Attributes)
            {
                // Check it's not a reserved property name
                if (!InternalProperties.Contains(pair.Name))
                {
                    var normalizedName = pair.Name.ToPascalCaseDash();

                    if (!NormalizedProperties.ContainsKey(normalizedName))
                    {
                        NormalizedProperties.Add(normalizedName, pair.Value.ToString());
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

            // Right here, we have to find the right type.
            // Should this be an override, where part and field have meaning, that is relevant to them.

            // if (options.Lookup("HtmlBodyComponent"))
            // {
            //     var s = await _shapeFactory.CreateAsync(type, lookupInitializer)
            // }
            var shape = await BuildComponentAsync();


            // var shape = await _shapeFactory.CreateAsync(Type, Arguments.From(properties));

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

            if (!tagHelperContext.Items.ContainsKey(typeof(IShape)))
            {
                tagHelperContext.Items.Add(typeof(IShape), shape);

            }


            if (!string.IsNullOrWhiteSpace(Cache))
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

            await output.GetChildContentAsync();

            if (!String.IsNullOrEmpty(Slot))
            {
                dynamic parentShape = tagHelperContext.Items[typeof(IShape)];
                if (parentShape != null)
                {
                    parentShape[Slot] = shape;

                }

                // tagHelperContext.Items.Add(Slot, await _displayHelper.ShapeExecuteAsync(shape));
            }
            else
            {
                output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));
            }
            // We don't want any encapsulating tag around the shape
            output.TagName = null;
        }
    }
}
