using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidPage : Razor.RazorPage<dynamic>
    {
        public override Task ExecuteAsync()
        {
            var viewContextAccessor = Context.RequestServices.GetRequiredService<ViewContextAccessor>();

            if (viewContextAccessor.ViewContext == null)
            {
                viewContextAccessor.ViewContext = ViewContext;
            }

            if (RenderAsync != null && ViewContext.ExecutingFilePath == LiquidViewsFeatureProvider.DefaultRazorViewPath)
            {
                return RenderAsync(ViewContext.Writer);
            }

            return LiquidViewTemplate.RenderAsync(this);
        }

        public System.Func<TextWriter, Task> RenderAsync { get; set; }
    }
}
