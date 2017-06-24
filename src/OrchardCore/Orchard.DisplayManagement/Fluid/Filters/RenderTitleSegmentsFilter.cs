using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Orchard.Liquid;

namespace Orchard.DisplayManagement.Fluid.Filters
{
    public class RenderTitleSegmentsFilter : ITemplateContextHandler
    {
        public void OnTemplateProcessing(TemplateContext context)
        {   
            context.Filters.AddFilter("render_title_segments", (input, arguments, ctx) =>
            {
                if (!ctx.AmbientValues.TryGetValue("FluidView", out var view) && view is FluidView)
                {
                    throw new ParseException("FluidView missing while invoking 'render_title_segments'.");
                }

                var position = "0";
                if (arguments.Names.Contains("position"))
                {
                    position = arguments["position"].ToStringValue();
                }

                IHtmlContent separator = null;
                if (arguments.Names.Contains("separator"))
                {
                    separator = new HtmlString(arguments["separator"].ToStringValue());
                }

                StringValue content;
                using (var writer = new StringWriter())
                {
                    (view as FluidView).RenderTitleSegments(new HtmlString(input.ToStringValue()),
                        position, separator).WriteTo(writer, HtmlEncoder.Default);

                    content = new StringValue(writer.ToString());
                    content.Encode = false;
                }

                return content;
            });
        }
    }
}
