using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class ClearAlternates : Statement
    {
        public override Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var model = (dynamic)context.LocalScope.GetValue("Model").ToObjectValue();

            if (model is IShape && model.Metadata?.Alternates?.Count > 0)
            {
                ((ShapeMetadata)model.Metadata).Alternates.Clear();
            }

            return Task.FromResult(Completion.Normal);
        }
    }
}