using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ClearAttributesTag : ExpressionTag
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression)
        {
            var objectValue = (await expression.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape && shape.Attributes.Count > 0)
            {
                shape.Attributes.Clear();
            }

            return Completion.Normal;
        }
    }
}