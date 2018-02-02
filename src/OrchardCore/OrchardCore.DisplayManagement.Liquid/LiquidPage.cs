using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidPage : Razor.RazorPage<dynamic>
    {
        public override async Task ExecuteAsync()
        {
            await LiquidViewTemplate.RenderAsync(this);
        }
    }
}
