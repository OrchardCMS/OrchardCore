using System;

namespace OrchardCore.Modules.Manifest
{
    /// <summary>
    /// Maps a module asset to its project location while in debug mode, auto generated on building.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class ModuleAssetAttribute : Attribute
    {
        public ModuleAssetAttribute(string asset)
        {
            Asset = asset ?? String.Empty;
        }

        /// <Summary>
        /// A module asset in the form of '{ModuleAssetPath}|{ProjectAssetPath}'.
        /// </Summary>
        public string Asset { get; }
    }
}
