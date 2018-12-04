using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidPage : Razor.RazorPage<dynamic>
    {
        public override Task ExecuteAsync()
        {
            if (ViewContext.ExecutingFilePath == LiquidViewsFeatureProvider.DefaultLiquidViewTemplateName
                + RazorViewEngine.ViewExtension && RenderAsync != null)
            {
                var viewContextAccessor = Context.RequestServices.GetRequiredService<ViewContextAccessor>();
                viewContextAccessor.ViewContext = ViewContext;

                return RenderAsync(ViewContext.Writer);
            }

            return LiquidViewTemplate.RenderAsync(this);
        }

        public System.Func<TextWriter, Task> RenderAsync { get; set; }
    }
}
