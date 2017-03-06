namespace Orchard.Environment.Extensions.Features
{
    public interface IFeatureInfo
    {
        string Id { get; }
        string Name { get; }
        int Priority { get; }
        string Category { get; }
        string Description { get; }
        IExtensionInfo Extension { get; }
        string[] Dependencies { get; }
    }
}
