namespace Orchard.Environment.Extensions.Info
{
    public interface IExtensionProvider
    {
        IExtensionInfo GetExtensionInfo(string subPath);
    }
}
