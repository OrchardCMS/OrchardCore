using System;

namespace OrchardCore.Modules.Manifest
{
    /// <summary>
    /// Marks an assembly as a module of a given type, auto generated on building.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ModuleMarkerAttribute : ModuleAttribute
    {
        private string _type;

        public ModuleMarkerAttribute(string name, string type)
        {
            Name = name;
            _type = type;
        }

        public override string Type => _type;
    }
}
