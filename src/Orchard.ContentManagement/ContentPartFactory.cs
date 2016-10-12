using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Orchard.ContentManagement;

public class ContentPartFactory : IContentPartFactory
{
    private readonly ConcurrentDictionary<string, Type> _contentPartTypes;

    public ContentPartFactory(IEnumerable<ContentPart> contentParts)
    {
        _contentPartTypes = new ConcurrentDictionary<string, Type>();

        foreach(var contentPart in contentParts)
        {
            _contentPartTypes.TryAdd(contentPart.GetType().Name, contentPart.GetType());
        }
    }

    public ContentPart CreateContentPart(string partName)
    {
        var type = GetContentPartType(partName);
        if (type != null)
        { 
            return (ContentPart)Activator.CreateInstance(type);
        }

        return null;
    }

    public Type GetContentPartType(string partName)
    {
        Type result = null;
        _contentPartTypes.TryGetValue(partName, out result);

        return result;
    }
}