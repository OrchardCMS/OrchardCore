namespace OrchardCore.Environment.Shell.Events
{
    public interface IShellEvents
    {
        /// <summary>
        /// The <see cref="ShellsEvent"/> delegate that will get called before loading all shells.
        /// </summary>
        ShellsEvent LoadingAsync { get; set; }

        /// <summary>
        /// The <see cref="ShellEvent"/> delegate that will get called before releasing a given shell.
        /// </summary>
        ShellEvent ReleasingAsync { get; set; }

        /// <summary>
        /// The <see cref="ShellEvent"/> delegate that will get called before reloading a given shell.
        /// </summary>
        ShellEvent ReloadingAsync { get; set; }

        /// <summary>
        /// The <see cref="ShellEvent"/> delegate that will get called before removing a given shell.
        /// </summary>
        ShellEvent RemovingAsync { get; set; }
    }
}
