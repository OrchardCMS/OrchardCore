using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;

public class ContentPartFactory : IContentPartFactory
{
    private readonly IEnumerable<ContentPart> _contentParts;
    
    public ContentPartFactory(IEnumerable<ContentPart> contentParts)
    {
        _contentParts = contentParts;
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
        return _contentParts.FirstOrDefault(x => x.GetType().Name == partName)?.GetType();
    }
}