using Orchard.ContentManagement;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.MetaWeblog;
using System;

namespace Orchard.Media.RemotePublishing
{
    public class MediaMetaWeblogDriver : MetaWeblogDriver
    {
        public override void SetCapabilities(Action<string, string> setCapability)
        {
            setCapability("supportsFileUpload", "Yes");
        }
    }
}
