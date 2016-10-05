using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Info.Manifests;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orchard.Environment.Extensions.Info
{
    public class ManifestProvider : IManifestProvider
    {
        private const string ManifestFile = "module.txt";

        private IFileProvider _fileProvider;

        public ManifestProvider(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public IManifestInfo GetManifest(string subPath)
        {
            var manifestFileInfo = _fileProvider.GetFileInfo(subPath);

            if (!manifestFileInfo.Exists)
            {
                return new NotFoundManifestFile(manifestFileInfo);
            }

            var attributes = ParseManifest(manifestFileInfo);

            return new ManifestInfo(
                manifestFileInfo,
                attributes["name"],
                attributes["description"],
                attributes);
        }

        // (ngm) should we hook this in to the yaml parser?
        private static IDictionary<string, string> ParseManifest(IFileInfo manifestFileInfo)
        {
            var manifest = new Dictionary<string, string>();

            using (Stream stream = manifestFileInfo.CreateReadStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] field = line.Split(new[] { ":" }, 2, StringSplitOptions.None);
                        int fieldLength = field.Length;
                        if (fieldLength != 2)
                            continue;
                        for (int i = 0; i < fieldLength; i++)
                        {
                            field[i] = field[i].Trim();
                        }

                        manifest.Add(field[0].ToLowerInvariant(), field[1]);
                    }
                }
            }

            return manifest;
        }
    }
}
