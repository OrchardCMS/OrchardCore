using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Model;

namespace Orchard.Core.Title.Drivers
{
    public class TitlePartDriver : ContentPartDriver<TitlePart>
    {
        protected override void GetContentItemMetadata(ContentItemMetadataContext context, TitlePart part)
        {
            context.Metadata.DisplayText = part.Title;
        }
    }
}