using System;

namespace OrchardCore.Environment.Extensions.Features.Attributes
{
    /// <summary>
    /// Allows the annotated class to be registered instead of the class being overridden.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]

    public class ServiceOverrideAttribute : ServiceImplAttribute
    {
        public ServiceOverrideAttribute(Type type) : this(type.FullName)
        {
        }

        public ServiceOverrideAttribute(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; }
    }
}