using System.Text.Json.Nodes;
using Cysharp.Text;
using Fluid;
using Fluid.Filters;
using Fluid.Values;
using Microsoft.Extensions.Hosting;
using OrchardCore.DisplayManagement;
using OrchardCore.Liquid;

namespace OrchardCore.ContentManagement.Display.Liquid;

public class ConsoleLogFilter : ILiquidFilter
{
    private readonly IHostEnvironment _hostEnvironment;

    public ConsoleLogFilter(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        var content = input.ToObjectValue();

        if (content == null || _hostEnvironment.IsProduction())
        {
            return NilValue.Instance;
        }

        using var sb = ZString.CreateStringBuilder();
        sb.Append("<script>console.log(");

        if (content is string stringContent)
        {
            sb.Append("\"");
            sb.Append(stringContent);
            sb.Append("\"");
        }
        else if (content is JsonNode jTokenContent)
        {
            sb.Append(jTokenContent.ToString());
        }
        else if (content is ContentItem contentItem)
        {
            sb.Append(OrchardRazorHelperExtensions.ConvertContentItem(contentItem).ToString());
        }
        else if (content is ContentPart contentPart)
        {
            sb.Append(OrchardRazorHelperExtensions.ConvertContentPart(contentPart).ToString());
        }
        else if (content is IShape shape)
        {
            sb.Append(shape.ShapeToJson().ToString());
        }
        else
        {
            sb.Append((await MiscFilters.Json(input, arguments, context)).ToStringValue());
        }

        sb.Append(")</script>");

        var result = new StringValue(sb.ToString(), false);

        return result;
    }
}
