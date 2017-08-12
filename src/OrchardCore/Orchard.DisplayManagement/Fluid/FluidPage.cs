using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidPage : Razor.RazorPage
    {
        public override async Task ExecuteAsync()
        {
            await FluidViewTemplate.RenderAsync(this);
        }
    }
}
