using System;
using System.Collections.Generic;

namespace OrchardCore.Modules.Manifest
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ModuleAttribute : Attribute
    {
        public ModuleAttribute(
            string name = null,
            string author = "",
            string website = "",
            string version = "0.0",
            string tags = "",
            string description = "",
            string dependencies = "",
            string priority = "0",
            string category = null)
        {
            Author = author;
            Website = website;
            Version = version;
            Tags = tags;

            Feature = new FeatureAttribute(id: null, name, description, dependencies, priority, category);
        }

        public virtual string Type => "Module";
        public bool Exists => Feature.Id != null;

        public string Author { get; }
        public string Website { get; }
        public string Version { get; }
        public string Tags { get; }

        public FeatureAttribute Feature { get; }
        public List<FeatureAttribute> Features { get; } = new List<FeatureAttribute>();
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class FeatureAttribute : Attribute
    {
        public FeatureAttribute(
            string id = null,
            string name = "",
            string description = "",
            string dependencies = "",
            string priority = "0",
            string category = null)
        {
            Id = id;
            Description = description;
            Dependencies = dependencies;
            Priority = priority;
            Category = category;
        }

        public string Id { get; set; }
        public string Name { get; }
        public string Description { get; }
        public string Dependencies { get; }
        public string Category { get; }
        public string Priority { get; }
    }
}