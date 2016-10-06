namespace Orchard.Environment.Extensions.Info
{
    public interface IFeatureInfo
    {
        string Id { get; }
        string Name { get; }
        string[] Dependencies { get; }
    }
}
