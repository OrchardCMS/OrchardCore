using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ZoneTag
    {
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, IReadOnlyList<Statement> statements, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext) context).Services;
            var layoutAccessor = services.GetRequiredService<ILayoutAccessor>();

            string position = null;
            string name = null;

            for (var i = 0; i < argumentsList.Count; i++)
            {
                var argument = argumentsList[i];
                // check common case
                if (argument.Name is null && argument.Expression is LiteralExpression literalExpression)
                {
                    name = literalExpression.Value.ToString();
                    continue;
                }

                switch (argument.Name)
                {
                    case "position":
                        position = (await argument.Expression.EvaluateAsync(context)).ToStringValue();
                        break;

                    case null:
                    case "name":
                    case "":
                        name ??= (await argument.Expression.EvaluateAsync(context)).ToStringValue();
                        break;
                }
            }

            if (statements != null && statements.Count > 0)
            {
                var content = new ViewBufferTextWriterContent();

                var completion = await statements.RenderStatementsAsync(content, encoder, context);

                if (completion != Completion.Normal)
                {
                    return completion;
                }

                var layout = await layoutAccessor.GetLayoutAsync();

                var zone = layout.Zones[name];

                if (zone is Shape shape)
                {
                    await shape.AddAsync(content, position);
                }
            }

            return Completion.Normal;
        }
    }
}
