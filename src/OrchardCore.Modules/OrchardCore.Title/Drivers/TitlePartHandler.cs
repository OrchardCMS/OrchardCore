using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Title.Model;

namespace OrchardCore.Title.Drivers
{
    public class TitlePartHandler : ContentPartHandler<TitlePart>
    {
        public override Task LoadingAsync(LoadContentContext context, TitlePart instance)
        {
            return Task.CompletedTask;
        }

        public override Task LoadedAsync(LoadContentContext context, TitlePart instance)
        {
            return Task.CompletedTask;
        }
    }
}
