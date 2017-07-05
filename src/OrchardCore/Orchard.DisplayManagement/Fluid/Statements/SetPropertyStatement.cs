using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class SetPropertyStatement : Statement
    {
        public SetPropertyStatement(Expression obj, Expression name, Expression value)
        {
            Object = obj;
            Name = name;
            Value = value;
        }

        public Expression Object { get; }
        public Expression Name { get; }
        public Expression Value { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var obj = (dynamic)((await Object.EvaluateAsync(context)).ToObjectValue());

            if (obj != null)
            {
                var name = await Name.EvaluateAsync(context);
                var value = await Value.EvaluateAsync(context);
                obj[name.ToStringValue()] = value.ToObjectValue();
            }

            return Completion.Normal;
        }
    }
}