using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ResourceManagement;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ScriptBlock : ArgumentsBlock
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments, IList<Statement> statements)
        {
            var at = "Foot";

            foreach (var argument in arguments)
            {
                if (argument.Name != "at" && !string.IsNullOrEmpty(argument.Name))
                {
                    continue;
                }

                at = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
            }

            if (!context.AmbientValues.TryGetValue("Services", out var servicesObj))
            {
                throw new ArgumentException("Services missing while invoking 'cache' block");
            }

            var services = servicesObj as IServiceProvider;

            var resourceManager = services.GetService<IResourceManager>();

            using (var sw = new StringWriter())
            {
                await RenderStatementsAsync(sw, encoder, context, statements);

                if (String.Equals(at, "Head", StringComparison.OrdinalIgnoreCase))
                {
                    resourceManager.RegisterHeadScript(new HtmlString(sw.ToString()));
                }
                else
                {
                    resourceManager.RegisterFootScript(new HtmlString(sw.ToString()));
                }
            }                

            return Completion.Normal;
        }
    }
}
