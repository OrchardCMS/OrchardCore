namespace Orchard.Environment.Extensions.Info.Extensions
{
    public interface IExtensionProvider
    {
        IExtensionInfo GetExtensionInfo(string subPath);
    }
}
