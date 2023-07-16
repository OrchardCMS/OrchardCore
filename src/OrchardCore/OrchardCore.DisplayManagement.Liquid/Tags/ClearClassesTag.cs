using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ClearClassesTag
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public static async ValueTask<Completion> WriteToAsync(Expression expression, TextWriter writer, TextEncoder encoder, TemplateContext context)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var objectValue = (await expression.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape && shape.Classes.Count > 0)
            {
                shape.Classes.Clear();
            }

            return Completion.Normal;
        }
    }
}
