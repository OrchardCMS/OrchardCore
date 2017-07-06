using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Orchard.DisplayManagement.Fluid.Ast;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class RemoveItemStatement : Statement
    {
        public RemoveItemStatement(Expression shape, Expression name)
        {
            Shape = shape;
            Name = name;
        }

        public Expression Shape { get; }
        public Expression Name { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var shape = (await Shape.EvaluateAsync(context)).ToObjectValue() as Shape;
            var name = (await Name.EvaluateAsync(context)).ToStringValue();

            if (shape?.Items != null)
            {
                shape.Remove(name);
            }

            return Completion.Normal;
        }
    }
}