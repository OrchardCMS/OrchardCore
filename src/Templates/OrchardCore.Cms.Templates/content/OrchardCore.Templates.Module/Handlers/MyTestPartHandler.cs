using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;
using OrchardCore.Templates.Module.Models;

namespace OrchardCore.Templates.Module.Handlers
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