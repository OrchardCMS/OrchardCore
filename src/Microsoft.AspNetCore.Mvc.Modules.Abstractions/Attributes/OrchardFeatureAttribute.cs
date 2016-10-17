using System;

namespace Orchard
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OrchardFeatureAttribute : Attribute
    {
        public OrchardFeatureAttribute(string text)
        {
            FeatureName = text;
        }

        public string FeatureName { get; set; }
    }
}