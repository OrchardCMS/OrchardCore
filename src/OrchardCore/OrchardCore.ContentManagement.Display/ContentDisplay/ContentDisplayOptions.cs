using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentDisplayOptions
    {

        private readonly List<ContentPartDisplayOption> _contentParts = new List<ContentPartDisplayOption>();

        //TODO fields
        //private readonly List<ContentFieldOption> _contentFields = new List<ContentFieldOption>();


        public ContentPartDisplayOption TryAddContentPart(Type contentPartType)
        {
            if (!contentPartType.IsSubclassOf(typeof(ContentPart)))
            {
                throw new ArgumentException("The type must inherit from " + nameof(ContentPart));
            }
            var option = _contentParts.FirstOrDefault(x => x.Type == contentPartType);
            if (option == null)
            {
                option = new ContentPartDisplayOption(contentPartType);
                _contentParts.Add(option);
            }
            return option;
        }

        public void WithDisplayDriver(Type contentPartType, Type displayDriverType)
        {
            var option = _contentParts.FirstOrDefault(x => x.Type == contentPartType);
            option.WithDisplayDriver(displayDriverType);
        }

        //public ContentFieldOption AddContentField<T>() where T : ContentField
        //{
        //    return AddContentField(typeof(T));
        //}

        //public ContentFieldOption AddContentField(Type contentFieldType)
        //{
        //    if (!contentFieldType.IsSubclassOf(typeof(ContentField)))
        //    {
        //        throw new ArgumentException("The type must inherit from " + nameof(ContentField));
        //    }

        //    var option = new ContentFieldOption(contentFieldType);
        //    _contentFields.Add(option);

        //    return option;
        //}

        //public IReadOnlyList<ContentPartOption> ContentPartOptions => _contentParts;

        private Dictionary<string, ContentPartDisplayOption> _contentPartOptions;
        public IReadOnlyDictionary<string, ContentPartDisplayOption> ContentPartOptions => _contentPartOptions ??= _contentParts.ToDictionary(k => k.Type.Name);

        //public IReadOnlyList<ContentFieldOption> ContentFieldOptions => _contentFields;
    }
}
