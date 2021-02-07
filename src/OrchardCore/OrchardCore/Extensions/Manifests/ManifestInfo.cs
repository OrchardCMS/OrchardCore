using System.Collections.Generic;
using OrchardCore.Modules.Manifest;

namespace OrchardCore.Environment.Extensions.Manifests
{
    public class ManifestInfo : IManifestInfo
    {
        public ManifestInfo(ModuleAttribute moduleInfo)
        {
            ModuleInfo = moduleInfo;
        }

        public bool Exists => ModuleInfo.Exists;
        public string Name => ModuleInfo.Name ?? ModuleInfo.Id;
        public string Description => ModuleInfo.Description;
        public string Type => ModuleInfo.Type;
        public string Author => ModuleInfo.Author;
        public string Website => ModuleInfo.Website;
        public string Version => ModuleInfo.Version;
        public IEnumerable<string> Tags => ModuleInfo.Tags;
        public ModuleAttribute ModuleInfo { get; }
    }
}
