using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Title.Model;

namespace Orchard.Core.Title.Drivers
{
    public class TitlePartDriver : ContentPartDriver<TitlePart>
    {
        protected override void GetContentItemMetadata(TitlePart part, ContentItemMetadata metadata)
        {
            metadata.DisplayText = part.Title;
        }
    }
}