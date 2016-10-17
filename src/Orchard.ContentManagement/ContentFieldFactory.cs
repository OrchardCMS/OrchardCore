using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;

public class ContentFieldFactory : IContentFieldFactory
{
    private readonly IEnumerable<ContentField> _contentFields;
    
    public ContentFieldFactory(IEnumerable<ContentField> contentFields)
    {
        _contentFields = contentFields;
    }

    public ContentField CreateContentField(string FieldName)
    {
        var contentField = _contentFields.FirstOrDefault(x => x.GetType().Name == FieldName);
        if (contentField != null)
        { 
            return (ContentField)Activator.CreateInstance(contentField.GetType());
        }

        return null;
    }
}