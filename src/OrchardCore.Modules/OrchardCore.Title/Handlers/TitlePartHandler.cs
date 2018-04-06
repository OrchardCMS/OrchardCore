using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Title.Model;

namespace OrchardCore.Title.Handlers
{
    public class TitlePartHandler : ContentPartHandler<TitlePart>
    {
        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, TitlePart part)
        {
            context.For<ContentItemMetadata>(contentItemMetadata =>
            {
                contentItemMetadata.DisplayText = part.Title;
            });

            return Task.CompletedTask;
        }
    }
}