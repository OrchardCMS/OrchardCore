using System;

namespace OrchardCore.Modules
{
    /// <summary>
    /// An attribute that can associate a service or component with
    /// a specific feature by its name. This component will only
    /// be used if the feature is enabled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FeatureAttribute : Attribute
    {
        public FeatureAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        /// <summary>
        /// The name of the feature to assign the component to.
        /// </summary>
        public string FeatureName { get; set; }
    }
}
