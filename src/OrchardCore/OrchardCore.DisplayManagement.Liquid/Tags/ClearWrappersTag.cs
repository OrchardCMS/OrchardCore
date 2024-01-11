using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ClearWrappers
    {
        public static async ValueTask<Completion> WriteToAsync(Expression expression, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var objectValue = (await expression.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape)
            {
                shape.Metadata.Wrappers.Clear();
            }

            return Completion.Normal;
        }
    }
}
