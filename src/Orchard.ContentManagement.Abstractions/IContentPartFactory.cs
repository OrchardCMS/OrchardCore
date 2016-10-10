using System;

namespace Orchard.ContentManagement
{
    public interface IContentPartFactory
    {
        Type GetContentPartType(string partName);
        ContentPart CreateContentPart(string partName);
    }
}