namespace OrchardCore.Environment.Extensions.Features
{
    public interface IFeatureInfo
    {
        string Id { get; }
        string Name { get; }
        int Priority { get; }
        string Category { get; }
        string Description { get; }
        bool DefaultTenantOnly { get; }
        IExtensionInfo Extension { get; }
        string[] Dependencies { get; }
        bool IsAlwaysEnabled { get; }
        bool EnabledByDependencyOnly { get; }
    }
}
