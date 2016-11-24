namespace Orchard.Environment.Extensions.Loaders
{
    public interface IExtensionLoader
    {
        int Order { get; }
        string Name { get; }
        ExtensionEntry Load(IExtensionInfo extensionInfo);
    }
}
