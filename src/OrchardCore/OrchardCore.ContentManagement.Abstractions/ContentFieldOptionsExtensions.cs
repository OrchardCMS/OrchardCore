using System;
using System.Linq;

namespace OrchardCore.ContentManagement
{
    public static class ContentFieldOptionsExtensions
    {
        public static ContentFieldOptions AddField<TContentField>(this ContentFieldOptions contentFieldOptions)
            where TContentField : ContentField
        {
            var option = new ContentFieldOption<TContentField>();

            contentFieldOptions.FieldOptions = contentFieldOptions.FieldOptions.Union(new[] { option });

            return contentFieldOptions;
        }

        public static ContentFieldOptions AddField<TContentField>(this ContentFieldOptions contentFieldOptions, Action<ContentFieldOption> action)
            where TContentField : ContentField
        {
            var option = new ContentFieldOption<TContentField>();

            action(option);

            contentFieldOptions.FieldOptions = contentFieldOptions.FieldOptions.Union(new[] { option });

            return contentFieldOptions;
        }
    }
}
