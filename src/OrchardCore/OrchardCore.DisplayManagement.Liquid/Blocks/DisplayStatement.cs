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
    public class DisplayStatement : TagStatement
    {
        private static readonly HashSet<string> InternalProperties = new HashSet<string>
        {
            "shape", "slot"
        };

        private readonly ArgumentsExpression _arguments;

        public DisplayStatement(ArgumentsExpression arguments, List<Statement> statements = null) : base(statements)
        {
            _arguments = arguments;
        }

        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesObj))
            {
                throw new ArgumentException("Services missing while invoking 'shapeblock'");
            }

            var services = servicesObj as IServiceProvider;

            var shapeScopeManager = services.GetRequiredService<IShapeScopeManager>();
            var displayHelper = services.GetRequiredService<IDisplayHelper>();
            var shapeFactory = services.GetRequiredService<IShapeFactory>();
            var loggerFactory = services.GetRequiredService<ILogger<ShapeStatement>>();

            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var shape = arguments["shape"].ToObjectValue() as IShape;
            var slot = arguments["slot"].ToStringValue();



            shapeScopeManager.EnterScope(new ShapeScopeContext());

            if (shape != null)
            {
                shapeScopeManager.AddShape(shape);

            }

            // String content;

            try
            {
                // what happens here is it evaluates the other statements, to build child content, before executing the shape.
                await EvaluateStatementsAsync(encoder, context);

                var content = await displayHelper.ShapeExecuteAsync(shape);

                if (!string.IsNullOrEmpty(slot))
                {
                    shapeScopeManager.AddSlot(slot, content);

                }
                else
                {
                    var value = new HtmlContentValue(content);
                    value.WriteTo(writer, encoder, null);
                    // await writer.WriteAsync(new HtmlContentValue(content));

                }
            }
            finally
            {
                shapeScopeManager.ExitScope();
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
