using System;

namespace OrchardCore.Modules.Manifest
{
    /// <summary>
    /// Marks an assembly as a module of a given type, auto generated on building.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ModuleMarkerAttribute : ModuleAttribute
    {
        /// <summary>
        /// Constructs the attribute given <paramref name="id"/> and <paramref name="type"/>.
        /// </summary>
        /// <param name="id">The identifier for the Module.</param>
        /// <param name="type">Allows authors to specify a module specific Type.</param>
        public ModuleMarkerAttribute(
            string id,
            string type
        )
        {
            Id = id;
            Type = type;
        }
    }
}
