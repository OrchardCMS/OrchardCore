using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DisplayManagement.Blocks
{
    public class ShapeBlock : ArgumentsBlock
    {
        public override ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments, List<Statement> statements)
        {
            var exp = new ArgumentsExpression(arguments);
            var sta = new ShapeStatement(exp, statements);
            return sta.WriteToAsync(writer, encoder, context);
        }
    }
}
