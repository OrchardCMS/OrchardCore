using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidPage : Razor.RazorPage<dynamic>
    {
        public override Task ExecuteAsync() => LiquidViewTemplate.RenderAsync(this);
    }
}
