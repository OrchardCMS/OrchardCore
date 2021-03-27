using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
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
            var services = ((LiquidTemplateContext)context).Services;
            var layoutAccessor = services.GetRequiredService<ILayoutAccessor>();

            string position = null;
            string name = null;

            foreach (var argument in argumentsList)
            {
                switch (argument.Name)
                {
                    case "position": position = (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;

                    case null:
                    case "name":
                    case "": name ??= (await argument.Expression.EvaluateAsync(context)).ToStringValue(); break;
                }
            }

            HtmlString content;
            using (var sb = StringBuilderPool.GetInstance())
            {
                using (var output = new StringWriter(sb.Builder))
                {
                    if (statements != null && statements.Count > 0)
                    {
                        var completion = await statements.RenderStatementsAsync(output, encoder, context);

                        if (completion != Completion.Normal)
                        {
                            return completion;
                        }
                    }

                    await output.FlushAsync();
                }

                content = new HtmlString(sb.Builder.ToString());
            }

            if (content != null)
            {
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
