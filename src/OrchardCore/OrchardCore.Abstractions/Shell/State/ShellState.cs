using System.Collections.Generic;

namespace OrchardCore.Environment.Shell.State
{
    /// <summary>
    /// Represents the transitive list of features a tenant is made of at a specific moment.
    /// It's used to differentiate new features from existing ones in order to trigger events like
    /// Installed/Unistalled compared to only Enabled/Disabled.
    /// </summary>
    public class ShellState
    {
        public List<ShellFeatureState> Features { get; } = new List<ShellFeatureState>();
    }
}
