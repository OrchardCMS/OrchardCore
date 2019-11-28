using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentDisplayOptions
    {
        private readonly List<ContentPartDisplayOption> _contentParts = new List<ContentPartDisplayOption>();
        private readonly List<ContentFieldDisplayOption> _contentFields = new List<ContentFieldDisplayOption>();

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

        public void WithPartDisplayDriver(Type contentPartType, Type displayDriverType)
        {
            var option = _contentParts.FirstOrDefault(x => x.Type == contentPartType);
            option.WithDisplayDriver(displayDriverType);
        }

        public ContentFieldDisplayOption TryAddContentField(Type contentFieldType)
        {
            if (!contentFieldType.IsSubclassOf(typeof(ContentField)))
            {
                throw new ArgumentException("The type must inherit from " + nameof(ContentField));
            }
            var option = _contentFields.FirstOrDefault(x => x.Type == contentFieldType);
            if (option == null)
            {
                option = new ContentFieldDisplayOption(contentFieldType);
                _contentFields.Add(option);
            }

            return option;
        }

        public void WithFieldDisplayDriver(Type contentFieldType, Type displayDriverType)
        {
            var option = _contentFields.FirstOrDefault(x => x.Type == contentFieldType);
            option.WithDisplayDriver(displayDriverType);
        }

        private Dictionary<string, ContentPartDisplayOption> _contentPartOptions;
        public IReadOnlyDictionary<string, ContentPartDisplayOption> ContentPartOptions => _contentPartOptions ??= _contentParts.ToDictionary(k => k.Type.Name);

        private Dictionary<string, ContentFieldDisplayOption> _contentFieldOptions;
        public IReadOnlyDictionary<string, ContentFieldDisplayOption> ContentFieldOptions => _contentFieldOptions ??= _contentFields.ToDictionary(k => k.Type.Name);
    }
}
