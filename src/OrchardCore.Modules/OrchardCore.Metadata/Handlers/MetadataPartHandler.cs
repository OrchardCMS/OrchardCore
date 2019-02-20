using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;
using OrchardCore.Metadata.Models;

namespace OrchardCore.Metadata.Handlers
{
    public class MetadataPartHandler : ContentPartHandler<MetadataPart>
    {
        public override Task InitializingAsync(InitializingContentContext context, MetadataPart part)
        {
            return Task.CompletedTask;
        }
    }
}