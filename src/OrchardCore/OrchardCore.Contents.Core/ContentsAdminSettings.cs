using System.Collections.Generic;

namespace OrchardCore.Contents.Core;

public class ContentsAdminSettings
{
    public HashSet<string> IgnorableStereotypes { get; } = new HashSet<string>();
}
