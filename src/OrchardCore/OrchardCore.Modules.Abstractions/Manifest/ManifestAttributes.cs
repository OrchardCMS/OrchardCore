using System;
using System.Collections.Generic;

namespace OrchardCore.Modules.Manifest
{
    /// <summary>
    /// Defines a Module which is composed of features.
    /// If the Module has only one default feature, it may be defined there.
    /// </summary>

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ModuleAttribute : Attribute
    {
        /// <param name="Name">
        /// Human-readable name of the module. If not provided, the assembly name will be used.
        /// </param>
        /// <param name="Author">The developer of the module.</param>
        /// <param name="Website">The URL for the website of the module developer.</param>
        /// <param name="Version">The version number of the module in SemVer format.</param>
        /// <param name="Tags">A comma-separated lists of tags for the module.</param>
        /// <param name="Description">A brief summary of what the default feature does.</param>
        /// <param name="Dependencies">
        /// A comma-separated list of features that the default feature depends on.
        /// So that its drivers / handlers are invoked after those of dependencies.
        /// </param>
        /// <param name="Priority">
        /// The priority of the default feature without breaking the dependencies order.
        /// higher is the priority, later the drivers / handlers are invoked.
        /// </param>
        /// <param name="Category">
        /// The group (by category) that the default feature belongs.
        /// If not provided, defaults to 'Uncategorized'.
        /// </param>

        public ModuleAttribute(
            string Name = null,
            string Author = "",
            string Website = "",
            string Version = "0.0",
            string Tags = "",
            string Description = "",
            string Dependencies = "",
            string Priority = "0",
            string Category = null)
        {
            author = Author;
            website = Website;
            version = Version;
            tags = Tags;

            Feature = new FeatureAttribute(Id: null, Name, Description, Dependencies, Priority, Category);
        }

        public virtual string Type => "Module";
        public bool Exists => Feature.id != null;

        public string author { get; }
        public string website { get; }
        public string version { get; }
        public string tags { get; }

        public FeatureAttribute Feature { get; }
        public List<FeatureAttribute> Features { get; } = new List<FeatureAttribute>();
    }

    /// <summary>
    /// Defines a Feature in a Module, can be used multiple times.
    /// If at least one Feature is defined, the Module default feature is ignored.
    /// </summary>

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class FeatureAttribute : Attribute
    {
        /// <param name="Id">The identifier of the feature.</param>
        /// <param name="Name">
        /// Human-readable name of the feature. If not provided, the identifier will be used.
        /// </param>
        /// <param name="Description">A brief summary of what the feature does.</param>
        /// <param name="Dependencies">A comma-separated list of features that the feature depends on.
        /// So that its drivers / handlers are invoked after those of dependencies.
        /// </param>
        /// <param name="Priority">
        /// The priority of the feature without breaking the dependencies order.
        /// higher is the priority, later the drivers / handlers are invoked.
        /// </param>
        /// <param name="Category">
        /// The group (by category) that the feature belongs.
        /// If not provided, defaults to 'Uncategorized'.
        /// </param>

        public FeatureAttribute(
            string Id = null,
            string Name = null,
            string Description = "",
            string Dependencies = "",
            string Priority = "0",
            string Category = null)
        {
            id = Id;
            name = Name;
            description = Description;
            dependencies = Dependencies;
            priority = Priority;
            category = Category;
        }

        public string id { get; set; }
        public string name { get; }
        public string description { get; }
        public string dependencies { get; }
        public string category { get; }
        public string priority { get; }
    }
}