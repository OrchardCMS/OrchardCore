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
    public class LinkTag
    {
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;
            var resourceManager = services.GetRequiredService<IResourceManager>();

            string src = null;
            string rel = null;
            string condition = null;
            string title = null;
            string type = null;
            bool? appendVersion = null;

            Dictionary<string, string> customAttributes = null;

            foreach (var argument in argumentsList)
            {
                switch (argument.Name)
                {
                    case "src": src = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "rel": rel = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "condition": condition = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "title": title = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "type": type = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                    case "append_version": appendVersion = (await argument.Expression.EvaluateAsync(context)).ToBooleanValue(); break;
                    default: (customAttributes ??= new Dictionary<string, string>())[argument.Name] = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                }
            }

            var linkEntry = new LinkEntry();

            if (!string.IsNullOrEmpty(src))
            {
                linkEntry.Href = src;
            }

            if (!string.IsNullOrEmpty(rel))
            {
                linkEntry.Rel = rel;
            }

            if (!string.IsNullOrEmpty(condition))
            {
                linkEntry.Condition = condition;
            }

            if (!string.IsNullOrEmpty(title))
            {
                linkEntry.Title = title;
            }

            if (!string.IsNullOrEmpty(type))
            {
                linkEntry.Type = type;
            }

            if (appendVersion.HasValue)
            {
                linkEntry.AppendVersion = appendVersion.Value;
            }

            if (customAttributes != null)
            {
                foreach (var attribute in customAttributes)
                {
                    linkEntry.SetAttribute(attribute.Key, attribute.Value);
                }
            }

            resourceManager.RegisterLink(linkEntry);

            return Completion.Normal;
        }
    }
}
