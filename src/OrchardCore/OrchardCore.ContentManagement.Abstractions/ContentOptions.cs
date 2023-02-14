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
        private readonly List<ContentPartOption> _contentParts = new();
        private readonly List<ContentFieldOption> _contentFields = new();

        private IReadOnlyDictionary<string, ContentPartOption> _contentPartOptionsLookup;
        public IReadOnlyDictionary<string, ContentPartOption> ContentPartOptionsLookup => _contentPartOptionsLookup ??= ContentPartOptions.ToDictionary(k => k.Type.Name);

        private IReadOnlyDictionary<string, ContentFieldOption> _contentFieldOptionsLookup;
        public IReadOnlyDictionary<string, ContentFieldOption> ContentFieldOptionsLookup => _contentFieldOptionsLookup ??= ContentFieldOptions.ToDictionary(k => k.Type.Name);

        public IReadOnlyList<ContentPartOption> ContentPartOptions => _contentParts;
        public IReadOnlyList<ContentFieldOption> ContentFieldOptions => _contentFields;

        internal void AddPartHandler(Type contentPartType, Type handlerType)
        {
            var option = GetOrAddContentPart(contentPartType);
            option.AddHandler(handlerType);
        }

        internal void RemovePartHandler(Type contentPartType, Type handlerType)
        {
            var option = GetOrAddContentPart(contentPartType);
            option.RemoveHandler(handlerType);
        }

        internal ContentPartOption GetOrAddContentPart(Type contentPartType)
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

        internal void AddFieldHandler(Type contentFieldType, Type handlerType)
        {
            var option = GetOrAddContentField(contentFieldType);
            option.AddHandler(handlerType);
        }

        internal void RemoveFieldHandler(Type contentFieldType, Type handlerType)
        {
            var option = GetOrAddContentField(contentFieldType);
            option.RemoveHandler(handlerType);
        }

        internal ContentFieldOption GetOrAddContentField(Type contentFieldType)
        {
            if (!contentFieldType.IsSubclassOf(typeof(ContentField)))
            {
                throw new ArgumentException("The type must inherit from " + nameof(ContentField));
            }

            var option = _contentFields.FirstOrDefault(x => x.Type == contentFieldType);
            if (option == null)
            {
                option = new ContentFieldOption(contentFieldType);
                _contentFields.Add(option);
            }

            return option;
        }
    }
}
