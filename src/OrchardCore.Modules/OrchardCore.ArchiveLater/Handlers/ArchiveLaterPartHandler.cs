using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ArchiveLater.Models;

namespace OrchardCore.ArchiveLater.Handlers
{
    // TODO: Rmove this one the PR is ready
    public class ArchiveLaterPartHandler : ContentPartHandler<ArchiveLaterPart>
    {
        public override Task UnpublishedAsync(PublishContentContext context, ArchiveLaterPart part)
        {
            part.ScheduledArchiveUtc = null;
            part.Apply();

            return Task.CompletedTask;
        }
    }
}
