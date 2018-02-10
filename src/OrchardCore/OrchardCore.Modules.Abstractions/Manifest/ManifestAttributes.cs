using System;
using System.Collections.Generic;

namespace OrchardCore.Modules.Manifest
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ModuleAttribute : Attribute
    {
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

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class FeatureAttribute : Attribute
    {
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