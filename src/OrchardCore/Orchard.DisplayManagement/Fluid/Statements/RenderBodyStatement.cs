using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class RenderBodyStatement : Statement
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "RenderBody");
            var htmlContent = await page.RenderBodyAsync();
            htmlContent.WriteTo(writer, page.HtmlEncoder);
            return Completion.Normal;
        }
    }
}