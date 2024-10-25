using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Liquid;

public class ResourcesTag
{
    public static async ValueTask<Completion> WriteToAsync(IReadOnlyList<FilterArgument> argumentsList, TextWriter writer, TextEncoder _, TemplateContext context)
    {
        var services = ((LiquidTemplateContext)context).Services;
        var processors = services.GetRequiredService<IEnumerable<IResourcesTagHelperProcessor>>();

        var processorContext = new ResourcesTagHelperProcessorContext(ResourceTagType.Footer, writer);

        foreach (var argument in argumentsList)
        {
            switch (argument.Name)
            {
                case "type":
                    var typeString = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
                    if (Enum.TryParse<ResourceTagType>(typeString, out var type))
                    {
                        processorContext = processorContext with { Type = type };
                    }

                    break;
            }
        }

        foreach (var processor in processors)
        {
            await processor.ProcessAsync(processorContext);
        }

        return Completion.Normal;
    }
}
