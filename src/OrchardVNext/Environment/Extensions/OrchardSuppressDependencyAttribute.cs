using System;

namespace OrchardVNext.Environment.Extensions {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class OrchardSuppressDependencyAttribute : Attribute {
        public OrchardSuppressDependencyAttribute(string fullName) {
            FullName = fullName;
        }

        public string FullName { get; set; }
    }
}