namespace Orchard.Environment.Extensions.Features
{
    public interface IFeatureInfo
    {
        string Id { get; }
        string Name { get; }
        double Priority { get; }
        IExtensionInfo Extension { get; }
        string[] Dependencies { get; }

        bool DependencyOn(IFeatureInfo feature);
    }
}
