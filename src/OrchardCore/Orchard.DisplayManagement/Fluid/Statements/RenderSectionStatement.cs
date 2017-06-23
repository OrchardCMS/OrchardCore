using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class RenderSectionStatement : Statement
    {
        public RenderSectionStatement(string sectionName, bool required)
        {
            SectionName = sectionName;
            Required = required;
        }

        public string SectionName { get; }
        public bool Required { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (context.AmbientValues.TryGetValue("FluidView", out var view) && view is FluidView)
            {
                await writer.WriteAsync((await (view as FluidView).RenderSectionAsync(SectionName, Required)).ToString());
            }
            else
            {
                throw new ParseException("FluidView missing while invoking 'rendersection'.");
            }

            return Completion.Normal;
        }
    }
}