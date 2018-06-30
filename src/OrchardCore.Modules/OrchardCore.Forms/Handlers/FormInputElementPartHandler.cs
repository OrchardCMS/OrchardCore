using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.Handlers
{
    public class FormInputElementPartHandler : ContentPartHandler<FormInputElementPart>
    {
        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, FormInputElementPart part)
        {
            context.For<ContentItemMetadata>(contentItemMetadata =>
            {
                contentItemMetadata.DisplayText = part.Name;
            });

            return Task.CompletedTask;
        }
    }
}