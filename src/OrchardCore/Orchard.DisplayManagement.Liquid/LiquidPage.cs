using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Orchard.DisplayManagement.Liquid
{
    public class LiquidPage : RazorPage<dynamic>
    {
        public override async Task ExecuteAsync()
        {
            await LiquidViewTemplate.RenderAsync(this);
        }
    }
}
