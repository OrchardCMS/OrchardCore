using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Modules.Manifest
{
    /// <summary>
    /// Defines a Module which is composed of features.
    /// If the Module has only one default feature, it may be defined there.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ModuleAttribute : FeatureAttribute
    {
        public ModuleAttribute()
        {
        }

        public virtual string Type => "Module";
        public new bool Exists => Id != null;

        /// <Summary>
        /// Logical id allowing a module project to change its 'AssemblyName' without
        /// having to update the code. If not provided, the assembly name will be used.
        /// </Summary>
        public new string Id { get; set; }

        /// <Summary>The name of the developer.</Summary>
        public string Author { get; set; } = String.Empty;

        /// <Summary>The URL for the website of the developer.</Summary>
        public string Website { get; set; } = String.Empty;

        /// <Summary>The version number in SemVer format.</Summary>
        public string Version { get; set; } = "0.0";

        /// <Summary>A list of tags.</Summary>
        public string[] Tags { get; set; } = Enumerable.Empty<string>().ToArray();

        public List<FeatureAttribute> Features { get; } = new List<FeatureAttribute>();
    }
}
