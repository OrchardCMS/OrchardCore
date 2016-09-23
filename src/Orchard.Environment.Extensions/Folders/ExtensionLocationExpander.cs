namespace Orchard.Environment.Extensions.Folders
{
    public interface IExtensionLocationExpander
    {
        string ExtensionType { get; }
        string ManifestName { get; }
        bool ManifestOptional { get; }
        string[] SearchPaths { get; }
    }

    public class ExtensionLocationExpander: IExtensionLocationExpander
    {
        public ExtensionLocationExpander(string extensionType, string[] searchPaths, string manifestName, bool manifestOptional = false)
        {
            ExtensionType = extensionType;
            SearchPaths = searchPaths;
            ManifestName = manifestName;
            ManifestOptional = manifestOptional;
        }

        public string ExtensionType { get; private set; }
        public string[] SearchPaths { get; private set; }
        public string ManifestName { get; private set; }
        public bool ManifestOptional { get; private set; }
    }
}