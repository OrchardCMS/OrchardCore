using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Templates.Cms.Module.Models;

namespace OrchardCore.Templates.Cms.Module.Handlers
{
    public class MyTestPartHandler : ContentPartHandler<MyTestPart>
    {
        public override Task InitializingAsync(InitializingContentContext context, MyTestPart part)
        {
            part.Show = true;

            return Task.CompletedTask;
        }
    }
}
