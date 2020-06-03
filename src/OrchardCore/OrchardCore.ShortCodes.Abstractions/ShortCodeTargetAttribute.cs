using System;

namespace OrchardCore.ShortCodes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ShortCodeTargetAttribute : Attribute
    {
        public ShortCodeTargetAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
