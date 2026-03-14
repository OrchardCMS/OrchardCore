using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
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
        var paths = field.Paths;

        await _attachedMediaFieldFileService.CopyNewFilesToContentItemDirAsync(paths, context.CloneContentItem);

        var mediaFieldNodePath = (field.Content.Node as JsonNode).GetNormalizedPath();
        var mediaFieldNode = (context.CloneContentItem.Content as JsonDynamicObject).SelectNode(mediaFieldNodePath);

        // Update the paths to the new ones.
        mediaFieldNode[nameof(MediaField.Paths)] = ToJsonArray(paths);
    }

    private static JsonArray ToJsonArray(string[] paths)
    {
        var jsonArray = new JsonArray();
        foreach (var path in paths)
        {
            jsonArray.Add(path);
        }

        return jsonArray;
    }
}
