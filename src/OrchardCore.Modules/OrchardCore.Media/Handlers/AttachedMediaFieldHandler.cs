using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Handlers;

public class AttachedMediaFieldHandler : ContentFieldHandler<MediaField>
{
    private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public AttachedMediaFieldHandler(AttachedMediaFieldFileService attachedMediaFieldFileService, IContentDefinitionManager contentDefinitionManager)
    {
        _attachedMediaFieldFileService = attachedMediaFieldFileService;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async override Task ClonedAsync(CloneContentFieldContext context, MediaField field)
    {
        var mediaFieldNodePath = (field.Content.Node as JsonNode).GetPath();
        var fieldName = mediaFieldNodePath.Split('.')[1];

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(context.CloneContentItem.ContentType);

        var partFieldDefinition = contentTypeDefinition.Parts
            .SelectMany(tpd => tpd.PartDefinition.Fields)
            .Single(pfd => pfd.FieldDefinition.Name == nameof(MediaField) && pfd.Name == fieldName);

        if (partFieldDefinition.Editor() == "Attached")
        {
            var updatedPaths = await _attachedMediaFieldFileService.CopyFilesAsync((string[])field.Paths.Clone(), context.CloneContentItem);

            var mediaFieldNode = (context.CloneContentItem.Content as JsonDynamicObject).SelectNode(mediaFieldNodePath);

            // Update the paths to the new ones.
            mediaFieldNode[nameof(MediaField.Paths)] = JArray.FromObject(updatedPaths);
        }
    }
}
