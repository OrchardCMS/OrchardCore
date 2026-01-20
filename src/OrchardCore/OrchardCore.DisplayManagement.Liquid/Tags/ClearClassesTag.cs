using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ClearClassesTag
    {
        public static async ValueTask<Completion> WriteToAsync(Expression expression, TextWriter _1, TextEncoder _2, TemplateContext context)
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
