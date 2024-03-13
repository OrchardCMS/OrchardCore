using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using OrchardCore.DisplayManagement.Liquid.Tags;

namespace OrchardCore.Contents.Liquid
{
    public class ContentItemTag
    {
        private static readonly FilterArgument _typeArgument = new("type", new LiteralExpression(StringValue.Create("contentitem")));

        public static ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            argumentsList.Add(_typeArgument);
            return ShapeTag.WriteToAsync(argumentsList, writer, encoder, context);
        }
    }
}
