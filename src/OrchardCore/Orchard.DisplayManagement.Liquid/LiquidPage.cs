using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidPage : RazorPage<dynamic>
    {
        public override async Task ExecuteAsync()
        {
            await LiquidViewTemplate.RenderAsync(this);
        }
    }
}
