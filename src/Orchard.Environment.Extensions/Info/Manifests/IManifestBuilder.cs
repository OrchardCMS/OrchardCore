using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Environment.Extensions.Info.Manifests
{
    public interface IManifestBuilder
    {
        IManifestInfo GetManifest(string subPath);
    }
}
