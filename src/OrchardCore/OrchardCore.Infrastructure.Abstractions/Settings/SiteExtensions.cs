using OrchardCore.Entities;

namespace OrchardCore.Settings;

public static class SiteExtensions
{
    public static ISite QueueReleaseShellContext(this ISite site)
    {
        if (!site.Properties.ContainsKey(nameof(ReleaseShellContextCommand)))
        {
            site.Put(ReleaseShellContextCommand.ReleaseInstance);
        }

        return site;
    }

    /// <summary>
    /// Removes the Reload property from the site.
    /// </summary>
    /// <param name="site"></param>
    /// <returns>true is the site should be reloaded otherwise false.</returns>
    public static bool DequeueReleaseShellContext(this ISite site)
    {
        // Get existing value before removing it.
        var reload = site.As<ReleaseShellContextCommand>().Release;

        site.Properties.Remove(nameof(ReleaseShellContextCommand));

        return reload;
    }

    private sealed class ReleaseShellContextCommand
    {
        public bool Release { get; set; }

        public ReleaseShellContextCommand()
        {

        }

        public static ReleaseShellContextCommand ReleaseInstance = new()
        {
            Release = true,
        };
    }
}
