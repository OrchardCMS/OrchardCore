using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions;

public class ExtensionDependencyStrategy : IExtensionDependencyStrategy
{
    public bool HasDependency(IFeatureInfo observer, IFeatureInfo subject)
    {
        return observer.Dependencies.Contains(subject.Id)
            || observer.After.Contains(subject.Id)
            || subject.Before.Contains(observer.Id);
    }
}
