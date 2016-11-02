using System;
using System.Linq;

namespace Orchard.Environment.Extensions.Features
{
    public static class FeatureExtensions
    {
        public static bool DependencyOn(this IFeatureInfo observer, IFeatureInfo subject)
        {
            return observer.Dependencies.Any(x =>
                StringComparer.OrdinalIgnoreCase.Equals(x, subject.Id));
        }
    }
}
