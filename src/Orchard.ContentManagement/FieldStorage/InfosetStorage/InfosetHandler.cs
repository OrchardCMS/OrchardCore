using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.FieldStorage.InfosetStorage {
    public class InfosetHandler : ContentHandlerBase {
        public override void Activating(ActivatingContentContext context) {
            context.Builder.Weld<InfosetPart>();
        }

        public override void Creating(CreateContentContext context) {
            var infosetPart = context.ContentItem.As<InfosetPart>();
            if (infosetPart != null) {
                context.ContentItemRecord.Data = infosetPart.Infoset.Data;
                context.ContentItemVersionRecord.Data = infosetPart.VersionInfoset.Data;

                infosetPart.Infoset = context.ContentItemRecord.Infoset;
                infosetPart.VersionInfoset = context.ContentItemVersionRecord.Infoset;
            }
        }
        public override void Loading(LoadContentContext context) {
            var infosetPart = context.ContentItem.As<InfosetPart>();
            if (infosetPart != null) {
                infosetPart.Infoset = context.ContentItemRecord.Infoset;
                infosetPart.VersionInfoset = context.ContentItemVersionRecord.Infoset;
            }
        }
        public override void Versioning(VersionContentContext context) {
            var infosetPart = context.BuildingContentItem.As<InfosetPart>();
            if (infosetPart != null) {
                infosetPart.Infoset = context.ContentItemRecord.Infoset;
                infosetPart.VersionInfoset = context.BuildingItemVersionRecord.Infoset;
            }
        }
    }
}