using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Title.Model;

namespace Orchard.Title.Handlers
{
    public class TitlePartHandler : ContentPartHandler<TitlePart>
    {
        public override void GetContentItemAspect(ContentItemAspectContext context, TitlePart part)
        {
            context.For<ContentItemMetadata>(contentItemMetadata =>
            {
                contentItemMetadata.DisplayText = part.Title;
            });
        }
    }
}