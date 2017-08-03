using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidPage : Razor.RazorPage
    {
        public static readonly string Content =
            "@inherits Orchard.DisplayManagement.Fluid.FluidPage" +
            System.Environment.NewLine + "@{ await base.ExecuteAsync(); }";

        public override async Task ExecuteAsync()
        {
            await FluidViewTemplate.RenderAsync(this);
        }

        public T GetService<T>()
        {
            return Context.RequestServices.GetService<T>();
        }
    }
}
