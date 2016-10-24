namespace Orchard.Environment.Extensions
{
    public interface IExtensionProvider
    {
        IExtensionInfo GetExtensionInfo(string subPath);
    }
}
