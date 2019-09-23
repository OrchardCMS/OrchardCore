using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Provides a way to register code based content parts and fields.
    /// </summary>
    public class ContentOptions
    {
        private readonly List<ContentPartOption> _contentParts = new List<ContentPartOption>();

        private readonly List<ContentFieldOption> _contentFields = new List<ContentFieldOption>();
        internal ContentPartOption AddContentPart<T>() where T : ContentPart
        {
            return AddContentPart(typeof(T));
        }

        internal ContentPartOption AddContentPart(Type contentPartType)
        {
            if (!contentPartType.IsSubclassOf(typeof(ContentPart)))
            {
                throw new ArgumentException("The type must inherit from " + nameof(ContentPart));
            }

            var option = new ContentPartOption(contentPartType);
            _contentParts.Add(option);

            return option;
        }

        internal ContentPartOption TryAddContentPart(Type contentPartType)
        {
            if (!contentPartType.IsSubclassOf(typeof(ContentPart)))
            {
                throw new ArgumentException("The type must inherit from " + nameof(ContentPart));
            }
            var option = _contentParts.FirstOrDefault(x => x.Type == contentPartType);
            if (option == null)
            {
                option = new ContentPartOption(contentPartType);
                _contentParts.Add(option);
            }
            return option;
        }

        internal void WithPartHandler(Type contentPartType, Type handlerType)
        {
            var option = _contentParts.FirstOrDefault(x => x.Type == contentPartType);
            option.WithHandler(handlerType);
        }

        internal ContentFieldOption AddContentField<T>() where T : ContentField
        {
            return AddContentField(typeof(T));
        }

        internal ContentFieldOption AddContentField(Type contentFieldType)
        {
            if (!contentFieldType.IsSubclassOf(typeof(ContentField)))
            {
                throw new ArgumentException("The type must inherit from " + nameof(ContentField));
            }

            var option = new ContentFieldOption(contentFieldType);
            _contentFields.Add(option);

            return option;
        }

        public IReadOnlyList<ContentPartOption> ContentPartOptions => _contentParts;

        private IReadOnlyDictionary<string, ContentPartOption> _contentPartOptionsLookup;
        public IReadOnlyDictionary<string, ContentPartOption> ContentPartOptionsLookup => _contentPartOptionsLookup ??= ContentPartOptions.ToDictionary(k => k.Type.Name);

        public IReadOnlyList<ContentFieldOption> ContentFieldOptions => _contentFields;
    }
}
