using System;

namespace OrchardCore.Modules
{
    public class Asset
    {
        public Asset(string asset)
        {
            asset = asset.Replace('\\', '/');
            var index = asset.IndexOf('|');

            if (index == -1)
            {
                ModuleAssetPath = String.Empty;
                ProjectAssetPath = String.Empty;
            }
            else
            {
                ModuleAssetPath = asset[..index];
                ProjectAssetPath = asset[(index + 1)..];
            }
        }

        public string ModuleAssetPath { get; }
        public string ProjectAssetPath { get; }
    }
}
