using System;
using System.Linq;

namespace OrchardCore.Modules.Manifest
{
    /// <summary>
    /// Defines a Feature in a Module, can be used multiple times.
    /// If at least one Feature is defined, the Module default feature is ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class FeatureAttribute : Attribute
    {
        public FeatureAttribute()
        {
        }

        public bool Exists => Id != null;

        /// <Summary>The identifier of the feature.</Summary>
        public string Id { get; set; }

        /// <Summary>
        /// Human-readable name of the feature. If not provided, the identifier will be used.
        /// </Summary>
        public string Name { get; set; }

        /// <Summary>A brief summary of what the feature does.</Summary>
        public string Description { get; set; } = String.Empty;

        /// <Summary>
        /// A list of features that the feature depends on.
        /// So that its drivers / handlers are invoked after those of dependencies.
        /// </Summary>
        public string[] Dependencies { get; set; } = Enumerable.Empty<string>().ToArray();

        /// <Summary>
        /// The priority of the feature without breaking the dependencies order.
        /// higher is the priority, later the drivers / handlers are invoked.
        /// </Summary>
        public string Priority { get; set; } = "0";

        /// <Summary>
        /// The group (by category) that the feature belongs.
        /// If not provided, defaults to 'Uncategorized'.
        /// </Summary>
        public string Category { get; set; }

        /// <summary>
        /// Set to <c>true</c> to only allow the Default tenant to enable / disable the feature.
        /// </summary>
        public bool DefaultTenantOnly { get; set; }

        /// <summary>
        /// Once enabled, check whether the feature can't be disabled. Defaults to false.
        /// </summary>
        public bool IsAlwaysEnabled { get; set; } = false;
    }
}
