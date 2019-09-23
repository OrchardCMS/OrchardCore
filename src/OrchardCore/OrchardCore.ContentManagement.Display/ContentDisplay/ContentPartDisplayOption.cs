using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentPartDisplayOption
    {
        private readonly List<Type> _displayDrivers = new List<Type>();
        public ContentPartDisplayOption(Type contentPartType)
        {
            if (contentPartType == null)
            {
                throw new ArgumentNullException(nameof(contentPartType));
            }

            Type = contentPartType;
        }

        public Type Type { get; }

        public IReadOnlyList<Type> DisplayDrivers => _displayDrivers;

        internal void WithDisplayDriver(Type type)
        {
            _displayDrivers.Add(type);
        }
    }
}
