using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidPage : RazorPage<dynamic>
    {
        public override async Task ExecuteAsync()
        {
            await FluidViewTemplate.RenderAsync(this);
        }
    }
}
