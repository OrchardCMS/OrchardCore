using System.Collections.Generic;

namespace Orchard.Environment.Shell.State
{
    /// <summary>
    /// Represents the transitive list of features a tenant is made of at a specific moment.
    /// </summary>
    public class ShellState
    {
        public IEnumerable<ShellFeatureState> Features { get; } = new List<ShellFeatureState>();
    }
}
