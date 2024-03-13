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

            var type = ResourceType.Footer;

            foreach (var argument in argumentsList)
            {
                switch (argument.Name)
                {
#pragma warning disable CA1806 // Do not ignore method results
                    case "type": Enum.TryParse((await argument.Expression.EvaluateAsync(context)).ToStringValue(), out type); break;
#pragma warning restore CA1806 // Do not ignore method results
                }
            }

            switch (type)
            {
                case ResourceType.Meta:
                    resourceManager.RenderMeta(writer);
                    break;

                case ResourceType.HeadLink:
                    resourceManager.RenderHeadLink(writer);
                    break;

                case ResourceType.Stylesheet:
                    resourceManager.RenderStylesheet(writer);
                    break;

                case ResourceType.HeadScript:
                    resourceManager.RenderHeadScript(writer);
                    break;

                case ResourceType.FootScript:
                    resourceManager.RenderFootScript(writer);
                    break;

                case ResourceType.Header:
                    resourceManager.RenderMeta(writer);
                    resourceManager.RenderHeadLink(writer);
                    resourceManager.RenderStylesheet(writer);
                    resourceManager.RenderHeadScript(writer);
                    break;

                case ResourceType.Footer:
                    resourceManager.RenderFootScript(writer);
                    break;

                default:
                    break;
            }

            return Completion.Normal;
        }

        public enum ResourceType
        {
            Meta,
            HeadLink,
            Stylesheet,
            HeadScript,
            FootScript,
            Header,
            Footer
        }
    }
}
