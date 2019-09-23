using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement
{
    public class ContentPartOption
    {
        private readonly List<KeyValuePair<Type, Type>> _resolverPairs = new List<KeyValuePair<Type, Type>>();
        public ContentPartOption(Type contentPartType)
        {
            if (contentPartType == null)
            {
                throw new ArgumentNullException(nameof(contentPartType));
            }

            Type = contentPartType;
        }

        public Type Type { get; }

        private ILookup<Type, Type> _resolvers;
        public ILookup<Type, Type> Resolvers => _resolvers ??= _resolverPairs.ToLookup(x => x.Key, x => x.Value);

        internal void WithResolver(Type key, Type type)
        {
            _resolverPairs.Add(KeyValuePair.Create(key, type));
        }
    }
}
