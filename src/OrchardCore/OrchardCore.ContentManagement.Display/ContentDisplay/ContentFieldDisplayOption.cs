using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentFieldDisplayOption : ContentFieldOptionBase
    {
        private readonly List<Type> _displayDrivers = new List<Type>();
        public ContentFieldDisplayOption(Type contentFieldType) : base(contentFieldType) { }

        public IReadOnlyList<Type> DisplayDrivers => _displayDrivers;

        internal void WithDisplayDriver(Type type)
        {
            _displayDrivers.Add(type);
        }
    }
}
