using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidPage : Razor.RazorPage
    {
        public override async Task ExecuteAsync()
        {
            await RenderAsync(this);
        }

        public static async Task RenderAsync(FluidPage page)
        {
            await FluidViewTemplate.RenderAsync(page);
        }

        public T GetService<T>()
        {
            return Context.RequestServices.GetService<T>();
        }

        [RazorInject]
        public IUrlHelper Url { get; private set; }

        [RazorInject]
        public IHtmlHelper Html { get; private set; }
 
        [RazorInject]
        public IViewComponentHelper Component { get; private set; }

        [RazorInject]
        public IModelExpressionProvider ModelExpressionProvider { get; private set; }
    }
}
