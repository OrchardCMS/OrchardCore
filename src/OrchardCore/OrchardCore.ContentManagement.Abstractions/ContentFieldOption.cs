using System;

namespace OrchardCore.ContentManagement
{
    public class ContentFieldOption<TContentField> : ContentFieldOption where TContentField : ContentField
    {
        public ContentFieldOption() : base(typeof(TContentField))
        {
        }
    }

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