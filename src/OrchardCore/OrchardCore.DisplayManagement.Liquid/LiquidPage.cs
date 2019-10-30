using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidPage : Razor.RazorPage<dynamic>
    {
        public override Task ExecuteAsync()
        {
            static async Task Awaited(ValueTask task)
            {
                await task;
            }

            if (RenderAsync != null && ViewContext.ExecutingFilePath == LiquidViewsFeatureProvider.DefaultRazorViewPath)
            {
                var task = RenderAsync(ViewContext.Writer);
                if (task.IsCompletedSuccessfully)
                {
                    return Task.CompletedTask;
                }
                return Awaited(task);
            }

            return LiquidViewTemplate.RenderAsync(this);
        }

        public System.Func<TextWriter, ValueTask> RenderAsync { get; set; }
    }
}
