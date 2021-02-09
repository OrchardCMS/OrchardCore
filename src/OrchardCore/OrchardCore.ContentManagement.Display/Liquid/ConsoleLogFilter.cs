using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement;
using OrchardCore.Liquid;

namespace OrchardCore.ContentManagement.Display.Liquid
{
    public static class ConsoleLogFilter
    {
        public static ValueTask<FluidValue> ConsoleLog(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var env = context.Services.GetRequiredService<IHostEnvironment>();

            var content = input.ToObjectValue();

            if (content == null || env.IsProduction())
            {
                return new ValueTask<FluidValue>(NilValue.Instance);
            }

            using (var sb = StringBuilderPool.GetInstance())
            {
                sb.Builder.Append("<script>console.log(");

                if (content is string stringContent)
                {
                    sb.Builder.Append("\"").Append(stringContent).Append("\"");
                }
                else if (content is JToken jTokenContent)
                {
                    sb.Builder.Append(jTokenContent.ToString());
                }
                else if (content is ContentItem contentItem)
                {
                    sb.Builder.Append(OrchardRazorHelperExtensions.ConvertContentItem(contentItem).ToString());
                }
                else if (content is IShape shape)
                {
                    sb.Builder.Append(shape.ShapeToJson().ToString());
                }
                else
                {
                    sb.Builder.Append(JsonConvert.SerializeObject(content));
                }

                sb.Builder.Append(")</script>");

                var result = new StringValue(sb.Builder.ToString(), false);

                return new ValueTask<FluidValue>(result);
            }
        }
    }
}
