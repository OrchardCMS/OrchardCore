using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class DefaultAnchorTag
    {
        public static ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, IReadOnlyList<Statement> statements, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;
            var anchorTags = services.GetRequiredService<IEnumerable<IAnchorTag>>();

            foreach (var anchorTag in anchorTags.OrderBy(x => x.Order))
            {
                if (anchorTag.Match(argumentsList))
                {
                    return anchorTag.WriteToAsync(argumentsList, statements, writer, encoder, (LiquidTemplateContext)context);
                }
            }

            return new ValueTask<Completion>(Completion.Normal);
        }
    }
}
