using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Handlers;

public class AttachedMediaFieldHandler : ContentFieldHandler<MediaField>
{
    private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;

    public AttachedMediaFieldHandler(AttachedMediaFieldFileService attachedMediaFieldFileService)
    {
        _attachedMediaFieldFileService = attachedMediaFieldFileService;
    }

    public async override Task ClonedAsync(CloneContentFieldContext context, MediaField field)
    {
        var paths = field.Paths?.Length == 0
            ? []
            : field.Paths;

        await _attachedMediaFieldFileService.CopyNewFilesToContentItemDirAsync(paths, context.CloneContentItem);
    }
}
