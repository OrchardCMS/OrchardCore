using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.PublishLater.Models;

namespace OrchardCore.PublishLater.Handlers
{
    public class PublishLaterPartHandler : ContentPartHandler<PublishLaterPart>
    {
        public override Task PublishedAsync(PublishContentContext context, PublishLaterPart part)
        {
            part.ScheduledPublishUtc = null;
            part.Apply();

            return Task.CompletedTask;
        }
    }
}
