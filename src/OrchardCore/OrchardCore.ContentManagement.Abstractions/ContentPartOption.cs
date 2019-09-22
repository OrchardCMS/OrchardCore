using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement
{
    public class ContentPartOption
    {
        private readonly Dictionary<string, Type> _factoryTypes = new Dictionary<string, Type>();
        public ContentPartOption(Type contentPartType)
        {
            if (contentPartType == null)
            {
                throw new ArgumentNullException(nameof(contentPartType));
            }

            Type = contentPartType;
        }

        public Type Type { get; }

        public IReadOnlyDictionary<string, Type> FactoryTypes => _factoryTypes;

        internal void WithFactoryType(string factoryName, Type type)
        {
            _factoryTypes.Add(factoryName, type);
        }
    }
}
