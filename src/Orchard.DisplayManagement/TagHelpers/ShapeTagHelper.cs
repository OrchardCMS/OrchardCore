using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.DisplayManagement.Implementation;

namespace Orchard.DisplayManagement.TagHelpers
{
    public abstract class BaseShapeTagHelper : TagHelper
    {
        private static readonly string[] InternalProperties = new[] { "type", "cache-id", "cache-context", "cache-dependency", "cache-tag", "cache-duration" };
        private static readonly char[] Separators = new[] { ',', ' ' };

        protected IShapeFactory _shapeFactory;
        protected IDisplayHelperFactory _displayHelperFactory;

        public string Type { get; set; }
        public string Cache { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Context { get; set; }
        public string Tag { get; set; }
        public string Dependency { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        protected BaseShapeTagHelper(IShapeFactory shapeFactory, IDisplayHelperFactory displayHelperFactory)
        {
            _shapeFactory = shapeFactory;
            _displayHelperFactory = displayHelperFactory;
        }

        public override async Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            var display = (DisplayHelper)_displayHelperFactory.CreateHelper(ViewContext);

            // Extract all attributes from the tag helper to
            var properties = output.Attributes
                .Where(x => !InternalProperties.Contains(x.Name))
                .ToDictionary(x => LowerKebabToPascalCase(x.Name), x => (object)x.Value.ToString())
                ;

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

            if (string.IsNullOrWhiteSpace(Dependency) && output.Attributes.ContainsName("cache-dependency"))
            {
                Dependency = Convert.ToString(output.Attributes["cache-dependency"].Value);
            }

            if (string.IsNullOrWhiteSpace(Tag) && output.Attributes.ContainsName("cache-tag"))
            {
                Tag = Convert.ToString(output.Attributes["cache-tag"].Value);
            }

            if (!Duration.HasValue && output.Attributes.ContainsName("cache-duration"))
            {
                TimeSpan timespan;
                if (TimeSpan.TryParse(Convert.ToString(output.Attributes["cache-duration"].Value), out timespan))
                {
                    Duration = timespan;
                }
            }

            var shape = _shapeFactory.Create(Type, Arguments.From(properties));

            if (!string.IsNullOrWhiteSpace(Cache))
            {
                var metadata = shape.Metadata;
                metadata.Cache(Cache);

                if (Duration.HasValue)
                {
                    metadata.Cache().During(Duration.Value);
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

                if (!string.IsNullOrWhiteSpace(Dependency))
                {
                    var dependency = Dependency.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddDependency(dependency);
                }
            }

            output.Content.SetHtmlContent(await display.ShapeExecuteAsync(shape));

            // We don't want any encapsulating tag around the shape
            output.TagName = null;
        }

        /// <summary>
        /// Converts foo-bar to FooBar
        /// </summary>
        private static string LowerKebabToPascalCase(string attribute)
        {
            attribute = attribute.Trim();
            bool nextIsUpper = true;
            var result = new StringBuilder();
            for (int i = 0; i < attribute.Length; i++)
            {
                var c = attribute[i];
                if (c == '-')
                {
                    nextIsUpper = true;
                    continue;
                }

                if (nextIsUpper)
                {
                    result.Append(c.ToString().ToUpper());
                }
                else
                {
                    result.Append(c);
                }

                nextIsUpper = false;
            }

            return result.ToString();
        }
    }
}