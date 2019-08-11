using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidPage : Razor.RazorPage<dynamic>
    {
        public override Task ExecuteAsync()
        {
            if (RenderAsync != null && ViewContext.ExecutingFilePath == LiquidViewsFeatureProvider.DefaultRazorViewPath)
            {
                return RenderAsync(ViewContext.Writer);
            }

            return LiquidViewTemplate.RenderAsync(this);
        }

        public System.Func<TextWriter, Task> RenderAsync { get; set; }
    }
}
