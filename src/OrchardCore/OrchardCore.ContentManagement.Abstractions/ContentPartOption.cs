using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement
{
    public class ContentPartOption : ContentPartOptionBase
    {
        private readonly List<Type> _handlers = new List<Type>();

        public ContentPartOption(Type contentPartType) : base(contentPartType) { }

        public IReadOnlyList<Type> Handlers => _handlers;

        public void WithHandler(Type type)
        {
            _handlers.Add(type);
        }
    }
}
