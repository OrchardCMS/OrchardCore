using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Liquid
{
    public class ResourcesTag
    {
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter writer, TextEncoder _, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;
            var resourceManager = services.GetRequiredService<IResourceManager>();

            var type = ResourceTagType.Footer;

            foreach (var argument in argumentsList)
            {
                switch (argument.Name)
                {
                    case "type":
                        var typeString = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
                        if (Enum.TryParse<ResourceTagType>(typeString, out var parsedType))
                        {
                            type = parsedType;
                        }
                        break;
                }
            }

            switch (type)
            {
                case ResourceTagType.Meta:
                    resourceManager.RenderMeta(writer);
                    break;

                case ResourceTagType.HeadLink:
                    resourceManager.RenderHeadLink(writer);
                    break;

                case ResourceTagType.Stylesheet:
                    resourceManager.RenderStylesheet(writer);
                    break;

                case ResourceTagType.HeadScript:
                    resourceManager.RenderHeadScript(writer);
                    break;

                case ResourceTagType.FootScript:
                    resourceManager.RenderFootScript(writer);
                    break;

                case ResourceTagType.Header:
                    resourceManager.RenderMeta(writer);
                    resourceManager.RenderHeadLink(writer);
                    resourceManager.RenderStylesheet(writer);
                    resourceManager.RenderHeadScript(writer);
                    break;

                case ResourceTagType.Footer:
                    resourceManager.RenderFootScript(writer);
                    break;

                default:
                    break;
            }

            return Completion.Normal;
        }
    }
}
