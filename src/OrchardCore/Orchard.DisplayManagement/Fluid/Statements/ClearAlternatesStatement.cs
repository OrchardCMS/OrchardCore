using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class ClearAlternates : Statement
    {
        public ClearAlternates(Expression shape)
        {
            Shape = shape;
        }

        public Expression Shape { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var shape = (await Shape.EvaluateAsync(context)).ToObjectValue() as IShape;

            if (shape?.Metadata.Alternates.Count > 0)
            {
                shape.Metadata.Alternates.Clear();
            }

            return Completion.Normal;
        }
    }
}