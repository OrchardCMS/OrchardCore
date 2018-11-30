using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidPage : Razor.RazorPage<dynamic>
    {
        public override Task ExecuteAsync()
        {
            var viewContextAccessor = Context.RequestServices.GetRequiredService<ViewContextAccessor>();
            viewContextAccessor.ViewContext = ViewContext;

            return LiquidViewTemplate.RenderAsync(this);
        }
    }
}
