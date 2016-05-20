using System;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public abstract class ContentDisplayDriver : DisplayDriver<ContentItem, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>, IContentDisplayDriver
    {
        public override string GeneratePrefix(ContentItem model)
        {
            return "";
        }
    }
}
