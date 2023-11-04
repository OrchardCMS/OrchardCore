using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer;
public class ContentHandlerOptions
{
    public readonly Dictionary<Type, List<Type>> ContentParts = new();

    public readonly Dictionary<Type, List<Type>> ContentFields = new();

    internal void AddPartHandler(Type contentPartType, Type handlerType)
    {
        var option = GetOrAddContentPart(contentPartType);

        if (!typeof(IContentPartImportHandler).IsAssignableFrom(handlerType))
        {
            throw new ArgumentException("The type must inherit from " + nameof(IContentPartImportHandler));
        }

        option.Add(handlerType);
    }

    internal void AddFieldHandler(Type contentFieldType, Type handlerType)
    {
        var option = GetOrAddContentField(contentFieldType);

        if (!typeof(IContentFieldImportHandler).IsAssignableFrom(handlerType))
        {
            throw new ArgumentException("The type must inherit from " + nameof(IContentFieldImportHandler));
        }

        option.Add(handlerType);
    }

    internal List<Type> GetOrAddContentPart(Type contentPartType)
    {
        if (!contentPartType.IsSubclassOf(typeof(ContentPart)))
        {
            throw new ArgumentException("The type must inherit from " + nameof(ContentPart));
        }

        if (!ContentParts.TryGetValue(contentPartType, out var handlers))
        {
            handlers = new List<Type>();
            ContentParts.Add(contentPartType, handlers);
        }

        return handlers;
    }

    internal List<Type> GetOrAddContentField(Type contentFieldType)
    {
        if (!contentFieldType.IsSubclassOf(typeof(ContentField)))
        {
            throw new ArgumentException("The type must inherit from " + nameof(ContentField));
        }

        if (!ContentFields.TryGetValue(contentFieldType, out var handlers))
        {
            handlers = new List<Type>();
            ContentFields.Add(contentFieldType, handlers);
        }

        return handlers;
    }
}
