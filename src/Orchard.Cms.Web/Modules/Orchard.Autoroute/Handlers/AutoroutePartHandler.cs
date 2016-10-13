using Orchard.ContentManagement.Handlers;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.Services;

namespace Orchard.Title.Handlers
{
    public class AutoroutePartHandler : ContentPartHandler<AutoroutePart>
    {
        private readonly IAutorouteEntries _entries;

        public AutoroutePartHandler(IAutorouteEntries entries)
        {
            _entries = entries;
        }

        public override void Published(PublishContentContext context, AutoroutePart instance)
        {
            _entries.AddEntry(instance.ContentItem.ContentItemId, instance.Path);
        }

        public override void Unpublished(PublishContentContext context, AutoroutePart instance)
        {
            _entries.RemoveEntry(instance.ContentItem.ContentItemId, instance.Path);
        }

        public override void Removed(RemoveContentContext context, AutoroutePart instance)
        {
            _entries.RemoveEntry(instance.ContentItem.ContentItemId, instance.Path);
        }
    }
}