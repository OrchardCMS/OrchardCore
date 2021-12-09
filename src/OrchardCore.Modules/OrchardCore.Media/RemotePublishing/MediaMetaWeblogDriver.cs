using System;
using OrchardCore.MetaWeblog;

namespace OrchardCore.Media.RemotePublishing
{
    public class MediaMetaWeblogDriver : MetaWeblogDriver
    {
        public override void SetCapabilities(Action<string, string> setCapability)
        {
            setCapability("supportsFileUpload", "Yes");
        }
    }
}
