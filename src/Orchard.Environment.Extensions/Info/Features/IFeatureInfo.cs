namespace Orchard.Environment.Extensions.Info
{
    public interface IFeatureInfo
    {
        string Id { get; }
        string Name { get; }
        double Priority { get; }
        IExtensionInfo Extension { get; }
        string[] Dependencies { get; }
    }
}
