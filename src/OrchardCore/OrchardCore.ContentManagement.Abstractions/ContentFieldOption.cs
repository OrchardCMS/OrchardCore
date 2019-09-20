using System;

namespace OrchardCore.ContentManagement
{
    public class ContentFieldOption
    {
        public ContentFieldOption(Type contentFieldType)
        {
            if (contentFieldType == null)
            {
                throw new ArgumentNullException(nameof(contentFieldType));
            }

            Type = contentFieldType;
        }

        public Type Type { get; }
    }
}
