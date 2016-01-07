using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Razor.TagHelpers;
using Orchard.DisplayManagement.Implementation;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("shape", Attributes = nameof(Type))]
    public class ShapeTagHelper : TagHelper
    {
        private static string[] InternalProperties = new[] { "type", "cache-id", "cache-context", "cache-dependency", "cache-tag", "cache-duration" };

        private readonly IShapeFactory _shapeFactory;
        private readonly IDisplayHelperFactory _displayHelperFactory;

        public string Type { get; set; }
        public string Cache { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Context { get; set; }
        public string Tag { get; set; }
        public string Dependency { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public ShapeTagHelper(IShapeFactory shapeFactory, IDisplayHelperFactory displayHelperFactory)
        {
            _shapeFactory = shapeFactory;
            _displayHelperFactory = displayHelperFactory;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).Wait();
        }

        public override async Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            var display = (DisplayHelper)_displayHelperFactory.CreateHelper(ViewContext);

            // Extract all attributes from the tag helper to
            var properties = output.Attributes
                .Where(x => !InternalProperties.Contains(x.Name))
                .ToDictionary(x => LowerKebabToPascalCase(x.Name), x => (object)x.Value.ToString())
                ;

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

            if (String.IsNullOrWhiteSpace(Dependency) && output.Attributes.ContainsName("cache-dependency"))
            {
                Dependency = Convert.ToString(output.Attributes["cache-dependency"].Value);
            }

            if (String.IsNullOrWhiteSpace(Tag) && output.Attributes.ContainsName("cache-tag"))
            {
                Tag = Convert.ToString(output.Attributes["cache-tag"].Value);
            }

            if (!Duration.HasValue && output.Attributes.ContainsName("cache-duration"))
            {
                TimeSpan timespan;
                if(TimeSpan.TryParse(Convert.ToString(output.Attributes["cache-duration"].Value), out timespan))
                {
                    Duration = timespan;
                }
            }

            var shape = _shapeFactory.Create(Type, Arguments.From(properties));
            
            if (!String.IsNullOrWhiteSpace(Cache))
            {
                var metadata = shape.Metadata;
                metadata.Cache(Cache);

                if(Duration.HasValue)
                {
                    metadata.Cache().During(Duration.Value);
                }

                if (!String.IsNullOrWhiteSpace(Context))
                {
                    var contexts = Context.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddContext(contexts);
                }

                if (!String.IsNullOrWhiteSpace(Tag))
                {
                    var tags = Tag.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddTag(tags);
                }

                if (!String.IsNullOrWhiteSpace(Dependency))
                {
                    var dependency = Dependency.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    metadata.Cache().AddDependency(dependency);
                }
            }

            output.Content.SetContent(await display.ShapeExecuteAsync(shape));

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
            for(int i=0; i<attribute.Length; i++)
            {
                var c = attribute[i];
                if(c == '-')
                {
                    nextIsUpper = true;
                    continue;
                }

                if(nextIsUpper)
                {
                    result.Append(c.ToString().ToUpper());
                }
                else
                {
                    result.Append(c);
                }

                nextIsUpper = false;
            }

            return "";
        }
    }
}