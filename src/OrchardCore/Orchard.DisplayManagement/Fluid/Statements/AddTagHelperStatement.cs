using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class AddTagHelperStatement : Statement
    {
        public AddTagHelperStatement(string name, string assembly)
        {
            Name = name;
            Assembly = assembly;
        }

        public string Name { get; }
        public string Assembly { get; }

        public override Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "AddTagHelper");
            TagHelperStatement.RegisterTagHelper(page, Name, Assembly);
            return Task.FromResult(Completion.Normal);
        }
    }
}