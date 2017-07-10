using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class AddTagHelperStatement : Statement
    {
        public AddTagHelperStatement(string name, string assemblyName)
        {
            Name = name;
            AssemblyName = assemblyName;
        }

        public string Name { get; }
        public string AssemblyName { get; }

        public override Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "AddTagHelper");
            TagHelperStatement.RegisterTagHelper(page, Name, AssemblyName);
            return Task.FromResult(Completion.Normal);
        }
    }
}