using System.Collections.Generic;

namespace OrchardCore.Setup.Events;

public class CompletedSetupContext
{
    public bool Success { get; set; }

    public IDictionary<string, string> Errors { get; set; }
}
