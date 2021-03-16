using System;

namespace OrchardCore.ContentManagement
{
    public abstract class ContentFieldOptionBase
    {
        public ContentFieldOptionBase(Type contentFieldType)
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
