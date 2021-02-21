using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Liquid
{
    public class ResourcesTag
    {
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;
            var resourceManager = services.GetRequiredService<IResourceManager>();
            var htmlEncoder = services.GetRequiredService<HtmlEncoder>();

            var type = ResourceType.Footer;

            foreach (var argument in argumentsList)
            {
                switch (argument.Name)
                {
                    case "type": Enum.TryParse((await argument.Expression.EvaluateAsync(context)).ToStringValue(), out type); break;
                }
            }

            var buffer = new HtmlContentBuilder();

            switch (type)
            {
                case ResourceType.Meta:
                    resourceManager.RenderMeta(buffer);
                    break;

                case ResourceType.HeadLink:
                    resourceManager.RenderHeadLink(buffer);
                    break;

                case ResourceType.Stylesheet:
                    resourceManager.RenderStylesheet(buffer);
                    break;

                case ResourceType.HeadScript:
                    resourceManager.RenderHeadScript(buffer);
                    break;

                case ResourceType.FootScript:
                    resourceManager.RenderFootScript(buffer);
                    break;

                case ResourceType.Header:
                    resourceManager.RenderMeta(buffer);
                    resourceManager.RenderHeadLink(buffer);
                    resourceManager.RenderStylesheet(buffer);
                    resourceManager.RenderHeadScript(buffer);
                    break;

                case ResourceType.Footer:
                    resourceManager.RenderFootScript(buffer);
                    break;

                default:
                    break;
            }

            buffer.WriteTo(writer, htmlEncoder);

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
