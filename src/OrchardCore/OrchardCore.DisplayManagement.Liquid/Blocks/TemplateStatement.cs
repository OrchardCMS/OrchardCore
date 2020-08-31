using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid.Ast;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Blocks
{
    public class TemplateStatement : TagStatement
    {

        private readonly ArgumentsExpression _arguments;

        public TemplateStatement(ArgumentsExpression arguments, List<Statement> statements = null) : base(statements)
        {
            _arguments = arguments;
        }

        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesObj))
            {
                throw new ArgumentException("Services missing while invoking 'template' block");
            }

            var services = servicesObj as IServiceProvider;

            var shapeScopeManager = services.GetRequiredService<IShapeScopeManager>();

            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var slot = arguments.At(0).ToStringValue();

            if (!String.IsNullOrEmpty(slot))
            {

                var content = await EvaluateStatementsAsync(encoder, context);

                shapeScopeManager.AddSlot(slot, content);
            }


            return Completion.Normal;
        }

        private async Task<string> EvaluateStatementsAsync(TextEncoder encoder, TemplateContext context)
        {
            using (var sb = StringBuilderPool.GetInstance())
            {
                using (var content = new StringWriter(sb.Builder))
                {
                    foreach (var statement in Statements)
                    {
                        await statement.WriteToAsync(content, encoder, context);
                    }

                    await content.FlushAsync();
                }

                return sb.Builder.ToString();
            }
        }
    }
}
