using Orchard.ContentManagement.Handlers;
using Orchard.Title.Model;

namespace Orchard.Title.Handlers
{
    public class TitlePartHandler : ContentPartHandler<TitlePart>
    {
        public override void GetContentItemMetadata(ContentItemMetadataContext context, TitlePart part)
        {
            context.Metadata.DisplayText = part.Title;
        }
    }
}