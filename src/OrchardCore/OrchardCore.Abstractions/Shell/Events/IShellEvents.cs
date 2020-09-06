namespace OrchardCore.Environment.Shell.Events
{
    public interface IShellEvents
    {
        /// <summary>
        /// The <see cref="ShellsEvent"/> delegate that will get called on <see cref="IShellHost.InitializeAsync"/>.
        /// </summary>
        ShellsEvent InitializingAsync { get; set; }

        /// <summary>
        /// The <see cref="ShellEvent"/> delegate that will get called on <see cref="IShellHost.ReleaseShellContextAsync"/>.
        /// </summary>
        ShellEvent ReleasingAsync { get; set; }

        /// <summary>
        /// The <see cref="ShellEvent"/> delegate that will get called on <see cref="IShellHost.ReloadShellContextAsync"/>.
        /// </summary>
        ShellEvent ReloadingAsync { get; set; }
    }
}
