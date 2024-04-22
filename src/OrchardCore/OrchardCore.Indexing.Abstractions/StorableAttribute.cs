using System;

namespace OrchardCore.Indexing
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StorableAttribute : Attribute
    {
        public StorableAttribute(Type valueType)
        {
            ValueType = valueType;
        }

        public Type ValueType { get; private set; }
    }
}
