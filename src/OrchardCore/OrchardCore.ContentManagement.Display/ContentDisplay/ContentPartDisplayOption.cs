using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentPartDisplayOption : ContentPartOptionBase
    {
        private readonly List<Type> _displayDrivers = new List<Type>();
        public ContentPartDisplayOption(Type contentPartType) : base(contentPartType) { }

        public IReadOnlyList<Type> DisplayDrivers => _displayDrivers;

        internal void WithDisplayDriver(Type type)
        {
            _displayDrivers.Add(type);
        }
    }
}
