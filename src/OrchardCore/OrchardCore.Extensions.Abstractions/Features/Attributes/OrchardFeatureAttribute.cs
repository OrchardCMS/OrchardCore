using System;

namespace OrchardCore.Extensions.Features.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OrchardFeatureAttribute : Attribute
    {
        public OrchardFeatureAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        public string FeatureName { get; set; }
    }
}