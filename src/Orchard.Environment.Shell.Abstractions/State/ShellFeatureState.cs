namespace Orchard.Environment.Shell.State
{
    /// <summary>
    /// Reprensents the state if a feature in a tenant.
    /// </summary>
    public class ShellFeatureState
    {
        public string Name { get; set; }
        public State InstallState { get; set; }
        public State EnableState { get; set; }

        public bool IsInstalled { get { return InstallState == State.Up; } }
        public bool IsEnabled { get { return EnableState == State.Up; } }
        public bool IsDisabled { get { return EnableState == State.Down || EnableState == State.Undefined; } }

        public enum State
        {
            Undefined,
            Rising,
            Up,
            Falling,
            Down,
        }
    }
}
