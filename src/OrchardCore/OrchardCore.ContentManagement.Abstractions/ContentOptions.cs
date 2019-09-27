using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Provides a way to register code based content parts and fields.
    /// </summary>
    public class ContentOptions
    {
        private readonly List<CodeContentTypeOption> _codeContentTypes = new List<CodeContentTypeOption>();
        private readonly List<ContentPartOption> _contentParts = new List<ContentPartOption>();
        private readonly List<ContentFieldOption> _contentFields = new List<ContentFieldOption>();

        public CodeContentTypeOption AddCodeContentType<T>() where T : CodeContentType
        {
            return AddCodeContentType(typeof(T));
        }

        public CodeContentTypeOption AddCodeContentType(Type codeContentType)
        {
            if (!codeContentType.IsSubclassOf(typeof(CodeContentType)))
            {
                throw new ArgumentException("The type must inherit from " + nameof(codeContentType));
            }

            var option = new CodeContentTypeOption(codeContentType);
            _codeContentTypes.Add(option);

            return option;
        }

        public ContentPartOption AddContentPart<T>() where T : ContentPart
        {
            return AddContentPart(typeof(T));
        }

        public ContentPartOption AddContentPart(Type contentPartType)
        {
            if (!contentPartType.IsSubclassOf(typeof(ContentPart)))
            {
                throw new ArgumentException("The type must inherit from " + nameof(ContentPart));
            }

            var option = new ContentPartOption(contentPartType);
            _contentParts.Add(option);

            return option;
        }

        public ContentFieldOption AddContentField<T>() where T : ContentField
        {
            return AddContentField(typeof(T));
        }

        public ContentFieldOption AddContentField(Type contentFieldType)
        {
            if (!contentFieldType.IsSubclassOf(typeof(ContentField)))
            {
                throw new ArgumentException("The type must inherit from " + nameof(ContentField));
            }

            var option = new ContentFieldOption(contentFieldType);
            _contentFields.Add(option);

            return option;
        }

        public IReadOnlyList<CodeContentTypeOption> CodeContentTypeOptions => _codeContentTypes;

        public IReadOnlyList<ContentPartOption> ContentPartOptions => _contentParts;

        public IReadOnlyList<ContentFieldOption> ContentFieldOptions => _contentFields;
    }
}
