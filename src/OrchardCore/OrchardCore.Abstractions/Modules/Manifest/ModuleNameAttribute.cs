using System;

namespace OrchardCore.Modules.Manifest
{
    /// <summary>
    /// Enlists the package or project name of a referenced module, auto generated on building.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class ModuleNameAttribute : Attribute
    {
        public ModuleNameAttribute(string name)
        {
            Name = name ?? String.Empty;
        }

        /// <Summary>
        /// The package or project name of the referenced module.
        /// </Summary>
        public string Name { get; }
    }
}
