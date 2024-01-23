using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement
{
    public class ContentFieldOption : ContentFieldOptionBase
    {
        private readonly List<Type> _handlers = new();

        public ContentFieldOption(Type contentFieldType) : base(contentFieldType)
        {
        }

        public IReadOnlyList<Type> Handlers => _handlers;

        internal void AddHandler(Type type)
        {
            _handlers.Add(type);
        }

        internal void RemoveHandler(Type type)
        {
            _handlers.Remove(type);
        }
    }
}
