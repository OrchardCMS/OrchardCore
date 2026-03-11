using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Infrastructure;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Handlers;

/// <summary>
/// Content Handler to delete files used on attached media fields once the content item is deleted.
/// </summary>
public class AttachedMediaFieldContentHandler : ContentHandlerBase
{
    private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public AttachedMediaFieldContentHandler(AttachedMediaFieldFileService attachedMediaFieldFileService, IContentDefinitionManager contentDefinitionManager)
    {
        _attachedMediaFieldFileService = attachedMediaFieldFileService;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public override async Task RemovedAsync(RemoveContentContext context)
    {
        if (context.NoActiveVersionLeft)
        {
            await _attachedMediaFieldFileService.DeleteContentItemFolderAsync(context.ContentItem);
        }
    }

    public async override Task ClonedAsync(CloneContentContext context)
    {
        var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(context.CloneContentItem.ContentType);

        var attachedMediaFields = typeDefinition.Parts.Select(tpd => tpd.PartDefinition)
            .Where(pd => pd.Fields
                .Any(pfd => pfd.FieldDefinition.Name == nameof(MediaField) && string.Equals(pfd.Editor(), "Attached", StringComparison.OrdinalIgnoreCase)));

        foreach (var attachedMediaField in attachedMediaFields)
        {
            var contentItemFolder = _attachedMediaFieldFileService.GetContentItemFolder(context.ContentItem);
            var clonedContentItemFolder = _attachedMediaFieldFileService.GetContentItemFolder(context.CloneContentItem);

            DirectoryWrapper.CopyDirectory(contentItemFolder, clonedContentItemFolder);
        }
    }
}
