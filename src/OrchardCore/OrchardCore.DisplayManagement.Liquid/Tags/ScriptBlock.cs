using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ResourceManagement;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ScriptBlock : ArgumentsBlock
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments, IList<Statement> statements)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesObj))
            {
                throw new ArgumentException("Services missing while invoking 'cache' block");
            }

            var services = servicesObj as IServiceProvider;

            var resourceManager = services.GetService<IResourceManager>();

            var builder = new TagBuilder("script");

            var at = "Foot";

            foreach (var argument in arguments)
            {
                if (argument.Name != "at" && !string.IsNullOrEmpty(argument.Name))
                {
                    continue;
                }

                at = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
            }

            using (var sw = new StringWriter())
            {
                await RenderStatementsAsync(sw, encoder, context, statements);

                builder.InnerHtml.AppendHtml(sw.ToString());
            }

            foreach (var argument in arguments)
            {
                if (argument.Name != "at" && !string.IsNullOrEmpty(argument.Name))
                {
                    var value = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
                    builder.Attributes.Add(argument.Name, value);
                }
            }

            // If no type was specified, define a default one
            if (!builder.Attributes.ContainsKey("type"))
            {
                builder.Attributes.Add("type", "text/javascript");
            }

            if (String.Equals(at, "Head", StringComparison.OrdinalIgnoreCase))
            {
                resourceManager.RegisterHeadScript(builder);
            }
            else
            {
                resourceManager.RegisterFootScript(builder);
            }

            return Completion.Normal;
        }
    }
}
